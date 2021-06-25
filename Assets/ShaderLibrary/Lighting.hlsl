#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

#include "Assets/ShaderLibrary/Surface.hlsl"
#include "Assets/ShaderLibrary/Light.hlsl"
#include "Assets/ShaderLibrary/BRDF.hlsl"

half3 GetLighting(Surface surface, BRDF brdf, Light light)
{
    return saturate(dot(surface.normalWS, light.direction)) * light.color * brdf.diffuse;
}

//重载方便处理多个光源
float3 GetLighting(Surface surface, BRDF brdf)
{
    half3 col;
    for(int i = 0; i < GetDirLightCount(); i++)
    {
        col += GetLighting(surface, brdf, GetDirLight(i));
    }
    return  col;
}

#endif