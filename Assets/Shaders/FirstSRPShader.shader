Shader "SRP/FirstSRPShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            //就问你unity是不是有病，专门漏这么一个宏。。。。。。好吧是我有病
            // #define UNITY_MATRIX_I_M unity_WorldToObject
            // #define UNITY_MATRIX_M unity_ObjectToWorld
            #include "Assets/ShaderLibrary/Input.hlsl"
            //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            //是个宏，为了避免有的平台不支持cbuffer(常量缓冲区)，SRP中只有其中的东西发生改变时，untiy才会发起一次SetPassCall
            CBUFFER_START(UnityPerMaterial)

            half4 _Tint;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            CBUFFER_END

            
            
            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2f vert (a2v v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : COLOR
            {
                half4 col = tex2D(_MainTex, i.uv);
                return col * _Tint;
            }
            ENDHLSL
        }
    }
}
