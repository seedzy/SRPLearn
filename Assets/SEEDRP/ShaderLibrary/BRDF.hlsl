#ifndef CUSTOM_BRDF_INCLUDE
#define CUSTOM_BRDF_INCLUDE

#include "Assets/SEEDRP/ShaderLibrary/Surface.hlsl"
#include "Assets/SEEDRP/ShaderLibrary/Common.hlsl"


/////////////////////////////////////////////////////////////////////////////////////////
///函数流程是按照从上到下初始化数据->得到最终颜色的
/////////////////////////////////////////////////////////////////////////////////////////



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
/// 获取1-反射率, 因为我们需要反射率越高反射的颜色越强，自身的颜色越弱
/// </summary>
float GetOneMinusReflectivity(float metallic)
{
    float range = 1 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

/// <summary>
/// 计算光照出射辐照度，记得能量守恒
/// 补：BRDF所代表的光照出射辐射量对应的就是量化后的颜色
/// 因此BRDF内的diff，spec变量实际上就对应了该种反射的颜色
/// </summary>
BRDF GetBRDF(Surface surface, bool applyAlphaToDiffuse = false)
{
    BRDF brdf;

    //中间值，金属度越高自身颜色越不明显
    //float oneMinusReflectivity = 1 - surface.metallic;
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    
    brdf.diffuse = surface.color * GetOneMinusReflectivity(surface.metallic);
    
    //透明度预乘，说实话没能理解实际作用
    if(applyAlphaToDiffuse)
    {
        brdf.diffuse *= surface.alpha;
    }
    
    //能量守恒????
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}


/// <summary>
/// 计算反射强度，同URP，简化版Cook-Torrance模型
/// 重新描述一下，specularStrength是根据一系列物体表面属性计算用于光照计算的反射强度
/// 同phong模型中手动设置的specular强度，但实际上反射强度源于人对物体粗糙度的感知
/// 因此这里实际暴露到面板的是物体的粗糙度取反的光滑程度，再通过物理公式计算得到最终像素点的specular强度
/// 而不是用一个值直接代替物理性计算
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

    float d2 = Square(NH2 * (R2 - 1) + 1.0001);

    float n = brdf.roughness * 4 + 2;

    return R2 / (d2 * max(0.1, LH2) * n);
}

/// <summary>
/// 获取直接光BRDF光照颜色
/// </summary>
half3 GetDirectLightBRDF(Surface surface, BRDF brdf, Light light)
{
    //记住这是强度再混合上颜色，别把spec强度和颜色搞混了
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}





#endif