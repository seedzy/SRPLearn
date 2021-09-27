Shader "SRP/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor("色调", Color) = (1,1,1,1)
        _Metallic("金属度", Range(0, 1)) = 1
        _Smoothness("光滑度", Range(0, 1)) = 0.5
        
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DstBlend", float) = 0
        
        [Toggle(_PREMULTIPLY_ALPHA)]_PremulAlpha("预乘Alpha", float) = 0
        [Toggle(_Alpha_Clip)]_AlphaClip("AlphaClip", float) = 0
    }
    SubShader
    {
        Tags { "LightMode" = "CustomLit" "RenderQueue" = "Transparent"}
        LOD 100

        Pass
        {
            Blend[_SrcBlend][_DstBlend]
            
            HLSLPROGRAM
            //设置着色器编译级别以支持更多现代功能
            //https://docs.unity3d.com/cn/2019.3/Manual/SL-ShaderCompileTargets.html
            #pragma target 3.5
            
            #pragma vertex vert
            #pragma fragment frag
            //定义一个开启透明度预乘的关键字
            #pragma shader_feature _PREMULTIPLY_ALPHA
            #pragma shader_feature _ALPHA_Clip
            
            #include "Assets/SEEDRP/ShaderLibrary/Lighting.hlsl"
            
            //SRP Batcher
            // CBUFFER_START(UnityPerMaterial)
            // float4 _MainTex_ST;
            // half4 _BaseColor;
            // CBUFFER_END

            //GPU instancing
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
            UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            float4 _MainTex_ST;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 nomral : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 wordPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };



            v2f vert (a2v v)
            {
                
                v2f o;
                //ToDo这个instancing还是多看几遍吧
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.nomral);
                o.wordPos = TransformObjectToWorld(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                //instancing需要通过这个宏来访问静态缓冲区的属性
                half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);

                half4 albedo = color * baseColor;

                float3 viewDir = normalize(_WordSpaceCameraPos - i.wordPos);

                //设置表面属性
                Surface surface;
                surface.positionWS = i.wordPos;
                surface.normalWS = normalize(i.normalWS);
                surface.color = albedo.rgb;
                surface.alpha = albedo.a;
                surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UntiyPerMaterial, _Metallic);
                surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UntiyPerMaterial, _Smoothness);
                surface.viewDir = viewDir;
                surface.depth = length(TransformWorldToView(i.wordPos));//-TransformWorldToView(i.wordPos).z;

                //获得BRDF属性
            #if defined(_PREMULTIPLY_ALPHA)
                BRDF brdf = GetBRDF(surface, true);
            #else
                BRDF brdf = GetBRDF(surface);
            #endif

            // #if defined(_ALPHA_Clip)
            //     clip()
                

                //通过BRDF结构获得最终光照
                half3 finCol = GetLighting(surface, brdf);
                
                return half4(finCol, surface.alpha);
            }
            ENDHLSL
        }
        
        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}
            ColorMask 0
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
            
            ENDHLSL
        }
    }
    
    
    //自定义shader面板
    CustomEditor "CustomShaderGUI"
}
