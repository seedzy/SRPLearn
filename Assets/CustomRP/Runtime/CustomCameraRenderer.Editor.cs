using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 只记录Editor相关，方便管理
/// </summary>
public partial class CustomCameraRenderer
{
    
#if UNITY_EDITOR
    /// <summary>
    /// 使用了不支持的shader时的替换材质
    /// </summary>
    private static Material errorMaterial;
    
    //不支持的shader pass列表
    private static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    
    
    /// <summary>
    /// 逐个绘制不支持SRP的shader
    /// </summary>
    private void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        
        DrawingSettings drawingSettings = new DrawingSettings()
        {
            //鬼能知道有这么个字段
            overrideMaterial = errorMaterial
        };
        drawingSettings.sortingSettings = new SortingSettings(_camera);
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;
        
        for (int i = 0; i < legacyShaderTagIds.Length; ++i)
        {
            //设置此次绘制所能使用的pass列表？？？？？
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            
        }
        
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        
    }
#endif
    
}
