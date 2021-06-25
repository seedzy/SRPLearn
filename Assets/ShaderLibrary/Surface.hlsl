#ifndef CUSTOM_SURFACE_INCLUDE
#define CUSTOM_SURFACE_INCLUDE

struct Surface
{
    float3 normalWS;
    half3 color;
    half alpha;
    float metallic;
    float smoothness;
    float3 viewDir;
};

#endif