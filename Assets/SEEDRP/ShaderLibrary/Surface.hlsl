#ifndef CUSTOM_SURFACE_INCLUDE
#define CUSTOM_SURFACE_INCLUDE

struct Surface
{
    float3 positionWS;
    half3  normalWS;
    half3  viewDir;
    half3  color;
    float  alpha;
    float  metallic;
    float  smoothness;
    
};

#endif