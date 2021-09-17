#ifndef CUSTOM_LIGHT_INCLUDE
#define CUSTOM_LIGHT_INCLUDE

#include "Input.hlsl"
#include "Shadow.hlsl"

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float3 _DirectionalLightColor[MAX_DIRECTIONAL_LIGHT_COUNT];
    float3 _DirectionalLightDirection[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

//基本上是模仿URP那套做的,不过光源改成限制了4个平行光
struct Light
{
    half3 color;
    half3 direction;
    float attenuation;
};

int GetDirLightCount()
{
    return _DirectionalLightCount;
}

/// <summary>
/// 获取指定索引的平行光的阴影数据
/// </summary>
DirectionalShadowData GetDirectionalLightShadowData(int lightIndex, ShadowData shadowData)
{
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
    data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
    return data;
}

/// <summary>
/// 获取指定索引的平行光
/// </summary>
Light GetDirLight(int index, Surface surface, ShadowData shadowData)
{
    Light light;
    light.color = _DirectionalLightColor[index];
    light.direction = _DirectionalLightDirection[index];

    DirectionalShadowData data = GetDirectionalLightShadowData(index, shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(data, surface);
    
    return light;
}







#endif