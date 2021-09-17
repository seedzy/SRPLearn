#ifndef CUSTOM_SHADOWS_INCLUDE
#define CUSTOM_SHADOWS_INCLUDE

#include "Input.hlsl"
#include "Surface.hlsl"
#include "Common.hlsl"


#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

//#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z <= 0.0 || shadowCoord.z >= 1.0

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
float4x4 _DirectionalShadowVPMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
int      _CascadeCount;
float4   _CascadeCullingSpheres[MAX_CASCADE_COUNT];
float    _MaxShadowDistance;
CBUFFER_END


struct DirectionalShadowData
{
    //阴影强度
    float strength;
    //图块序号
    int   tileIndex;
};
/// <summary>
/// 记录阴影级联数据
/// </summary>
struct ShadowData
{
    int cascadeIndex;
    int strength;
};

ShadowData GetShadowData(Surface surface)
{
    ShadowData data;

    int i;
    for(i = 0; i < _CascadeCount; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        //根据判断表面点与包围球的距离关系，决定该表面点阴影的取值
        float disSurfWithSph = DistanceSquared(surface.positionWS, sphere.xyz);
        if(disSurfWithSph < sphere.w)
            break;
    }

    //避免出现超出包围球范围的阴影
    data.strength = 1;
    if(i == _CascadeCount)
        data.strength = 0;
    
    data.cascadeIndex = i;
    return data;
}

float SamplerDirectionalShadowAtlas(float3 posLightSpace)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, posLightSpace);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surface)
{
    if(data.strength <= 0)
        return 1;
    
    //采样点的WSpos转到directionalLight的裁切空间去采样阴影图集，获取阴影值
    float3 position = mul(_DirectionalShadowVPMatrices[data.tileIndex], float4(surface.positionWS, 1)).xyz;
    float shadow = SamplerDirectionalShadowAtlas(position);
    return lerp(1, shadow, data.strength);
}


#endif