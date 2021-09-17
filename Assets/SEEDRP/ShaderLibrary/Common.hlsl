#ifndef CUSTOM_COMMON_INCLUDE
#define CUSTOM_COMMON_INCLUDE
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
/// <summary>
/// 求平方，别问，问就是pow太费
/// </summary>
float Square(float v)
{
    return v * v;
}

/// <summary>
/// 计算两点间距离的平方
/// </summary>
float DistanceSquared(float3 A, float3 B)
{
    return dot(A - B, A - B);
}

#endif