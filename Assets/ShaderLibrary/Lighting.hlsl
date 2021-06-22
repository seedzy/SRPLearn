#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

#include "Assets/ShaderLibrary/Surface.hlsl"
#include "Assets/ShaderLibrary/Light.hlsl"

float3 GetLighting(Surface surface, Light light)
{
    return saturate(dot(surface.normalWS, light.direction)) * light.color;
}

//为什么要写个重载
float3 GetLighting(Surface surface)
{
    return GetLighting(surface, GetMainLight());
}

#endif