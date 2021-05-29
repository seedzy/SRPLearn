using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
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

    /// <summary>
    /// 设置这个以及之后else的值是为了避免访问camera.name带来的内存分配，可是为什么不在player模式时去掉调试采样呢？？？
    /// </summary>
    private string SampleCBufferName {get;set;}
    
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

    private void DrawGizmos()
    {
        //面板控制是否开启
        if (Handles.ShouldRenderGizmos())
        {
            //GizmoSubset决定了Gizmos是否受到后处理影响，这里两种都绘制了
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }

    /// <summary>
    /// 把一些只显示在Game的几何物体也绘制到Scene，如UI
    /// </summary>
    void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            //让scene也能绘制UI的核心语句
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }

    /// <summary>
    /// 调试用的，根据摄像机名字去设置缓冲区名字
    /// </summary>
    void PrepareBuffer()
    {
        //这个profiler只是在编辑器查看访问camera的开销，player下没有影响
        Profiler.BeginSample("Editor Only");
        _commandBuffer.name = SampleCBufferName = _camera.name;
        Profiler.EndSample();
    }
    
#else
    const string SampleCBufferName = bufferName;
    
#endif
    
    
}
