#ifndef CUSTOM_INPUT_INCLUDE
#define CUSTOM_INPUT_INCLUDE

#include "UnityInput.hlsl"
#define UNITY_MATRIX_I_M   unity_WorldToObject
#define UNITY_MATRIX_M     unity_ObjectToWorld
#define UNITY_MATRIX_V     unity_MatrixV
#define UNITY_MATRIX_VP    unity_MatrixVP
#define UNITY_MATRIX_P     OptimizeProjectionMatrix(glstate_matrix_projection)



#endif