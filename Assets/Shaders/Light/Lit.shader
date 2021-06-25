Shader "SRP/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor("色调", Color) = (1,1,1,1)
        _Metallic("金属度", Range(0, 1)) = 1
        _Smoothness("光滑度", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "LightMode" = "CustomLit" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            //设置着色器编译级别以支持更多现代功能
            //https://docs.unity3d.com/cn/2019.3/Manual/SL-ShaderCompileTargets.html
            #pragma target 3.5
            
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/ShaderLibrary/Input.hlsl"
            #include "Assets/ShaderLibrary/Surface.hlsl"
            #include "Assets/ShaderLibrary/Lighting.hlsl"
            
            //SRP Batcher
            // CBUFFER_START(UnityPerMaterial)
            // float4 _MainTex_ST;
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
                
                o.pos = TransformObjectToHClip(v.vertex);
                o.normalWS = TransformObjectToWorldNormal(v.nomral);
                o.wordPos = TransformObjectToWorld(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                // sample the texture
                //half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                //instancing需要通过这个宏来访问静态缓冲区的属性
                half4 col = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);

                float3 viewDir = normalize(_WordSpaceCameraPos - i.wordPos);

                //设置表面属性
                Surface surface;
                surface.normalWS = i.normalWS;
                surface.color = col.rgb;
                surface.alpha = col.a;
                surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UntiyPerMaterial, _Metallic);
                surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UntiyPerMaterial, _Smoothness);
                surface.viewDir = viewDir;

                //获得BRDF属性
                BRDF brdf = GetBRDF(surface);

                //通过BRDF结构获得最终光照
                half3 finCol = GetLighting(surface, brdf);
                
                return half4(finCol, surface.alpha);
            }
            ENDHLSL
        }
    }
}
