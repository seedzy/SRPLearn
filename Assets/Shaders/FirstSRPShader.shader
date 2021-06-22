Shader "SRP/FirstSRPShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint("Color", Color) = (1,1,1,1)
        //自定义混合模式，(还能这么写！！！)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", float) = 0
        
        //括号里这个类型似乎没什么特殊要求，有0、1就行
        [Enum(Off , 0, On, 1)] _ZWrite("Z Write", int) = 1
    }
    SubShader
    {
        
        Pass
        {
            //Tags{"Queue" = "Transparent + 100"}
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //开启GPU instancing
            #pragma multi_compile_instancing

            //就问你unity是不是有病，专门漏这么一个宏。。。。。。好吧是我有病
            // #define UNITY_MATRIX_I_M unity_WorldToObject
            // #define UNITY_MATRIX_M unity_ObjectToWorld
            #include "Assets/ShaderLibrary/Input.hlsl"
            //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"


            //是个宏，为了避免有的平台不支持cbuffer(常量缓冲区)，SRP中只有其中的东西发生改变时，unity才会发起一次SetPassCall
            CBUFFER_START(UnityPerMaterial)

            half4 _Tint;
            
            float4 _MainTex_ST;

            CBUFFER_END

            sampler2D _MainTex;

            
            
            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                //通过顶点数据获取当前渲染对象的索引,此处先声明了一个ID(GPU instancing)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            v2f vert (a2v v)
            {
                //通过顶点输入结构获取其中渲染对象的索引ID并将其存储到其他实例依赖的全局静态变量中(GPU instancing)
                UNITY_SETUP_INSTANCE_ID(v);
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
