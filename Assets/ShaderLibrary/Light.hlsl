#ifndef CUSTOM_LIGHT_INCLUDE
#define CUSTOM_LIGHT_INCLUDE

#include "Assets/ShaderLibrary/Input.hlsl"

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    int _DirLightCount;
    float3 _DirLightColor[MAX_DIRECTIONAL_LIGHT_COUNT];
    float3 _DirLightDirection[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

//基本上是模仿URP那套做的,不过光源改成限制了4个平行光
struct Light
{
    half3 color;
    half3 direction;
};

int GetDirLightCount()
{
    return _DirLightCount;
}

/// <summary>
/// 获取指定索引的平行光
/// </summary>
Light GetDirLight(int index)
{
    Light light;
    light.color = _DirLightColor[index];
    light.direction = _DirLightDirection[index];
    return light;
}



#endif