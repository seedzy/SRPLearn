#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDE
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDE

#include "Assets/SEEDRP/ShaderLibrary/Input.hlsl"
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



v2f ShadowCasterPassVertex (a2v v)
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

void ShadowCasterPassFragment(v2f i)
{
    UNITY_SETUP_INSTANCE_ID(i);
    //float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    // #ifdef _CLIPPING
    // clip(color.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
}

#endif