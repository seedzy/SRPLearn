#ifndef CUSTOM_LIGHT_INCLUDE
#define CUSTOM_LIGHT_INCLUDE

#include "Assets/ShaderLibrary/Input.hlsl"

CBUFFER_START(_CustomLight)
    float3 _MainLightColor;
    float3 _MainLightDirection;
CBUFFER_END

//基本上是模仿URP那套做的
struct Light
{
    half3 color;
    half3 direction;
};

Light GetMainLight()
{
    Light light;
    light.color = _MainLightColor;
    light.direction = _MainLightDirection;
    return light;
}

#endif