#ifndef CUSTOM_SHADOWS_INCLUDE
#define CUSTOM_SHADOWS_INCLUDE

#include "Assets/SEEDRP/ShaderLibrary/Input.hlsl"
#include "Assets/SEEDRP/ShaderLibrary/Surface.hlsl"


#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z <= 0.0 || shadowCoord.z >= 1.0

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
float4x4 _DirectionalShadowVPMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct DirectionalShadowData
{
    //阴影强度
    float strength;
    int   tileIndex;
};

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