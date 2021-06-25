#ifndef CUSTOM_BRDF_INCLUDE
#define CUSTOM_BRDF_INCLUDE

#include "Assets/ShaderLibrary/Surface.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

/// <summary>
/// 电介质最小反射率,基本上就是常见物体最小的反射率
/// </summary>
#define MIN_REFLECTIVITY 0.04

struct BRDF
{
    half3 diffuse;
    half3 specular;
    float roughness;
};

/// <summary>
/// 获取反射率, 反射率越高越接近反射的颜色
/// </summary>
float GetReflectivity(float metallic)
{
    float range = 1 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

/// <summary>
/// 计算光照出射辐照度，记得能量守恒
/// </summary>
BRDF GetBRDF(Surface surface)
{
    BRDF brdf;

    //中间值，金属度越高自身颜色越不明显
    //float oneMinusReflectivity = 1 - surface.metallic;
    float perceptualRoughness = 
    
    brdf.diffuse = surface.color * GetReflectivity(surface.metallic);
    //能量守恒
    brdf.specular = surface.color - brdf.diffuse;
    brdf.roughness = 1;
    return brdf;
}





#endif