#ifndef CUSTOM_BRDF_INCLUDE
#define CUSTOM_BRDF_INCLUDE

#include "Assets/ShaderLibrary/Surface.hlsl"
#include "Assets/ShaderLibrary/Common.hlsl"
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
/// 计算反射强度，同URP，简化版Cook-Torrance模型
/// d = (Normal . HalfNormal)^2 * (Roughness^2 - 1) + 1.0001
/// Roughness^2/(d^2 * max(0.1, (LightDir . HalfNormal)^2)) * n
/// 
/// n代表4Roughness + 2
/// </summary>
float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    //半法线，忘了就回去抄书吧,这里鉴于公式有除法为避免除0用safeNormalize，最小只会化到0.001
    float3 halfNormal = SafeNormalize(surface.viewDir + light.direction);

    float NH2 = Square(saturate(dot(surface.normalWS, halfNormal)));

    float LH2 = Square(saturate(dot(light.direction, halfNormal)));

    float R2 = Square(brdf.roughness);

    float d2 = NH2 * (R2 - 1) + 1.0001;

    
}


/// <summary>
/// 计算光照出射辐照度，记得能量守恒
/// </summary>
BRDF GetBRDF(Surface surface)
{
    BRDF brdf;

    //中间值，金属度越高自身颜色越不明显
    //float oneMinusReflectivity = 1 - surface.metallic;
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    
    brdf.diffuse = surface.color * GetReflectivity(surface.metallic);
    //能量守恒
    brdf.specular = surface.color - brdf.diffuse;
    
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}





#endif