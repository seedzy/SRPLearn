using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 摄像机渲染管理类，用于统一管理摄像机渲染
/// </summary>
public partial class CustomCameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private static string _commandBufferName = "CameraRender";

    /// <summary>
    /// 经过摄像机剔除后剩余物体的信息结构
    /// </summary>
    private CullingResults _cullingResults;
    /// <summary>
    /// 一个pass为SRPDefaultUnlit的shaderID
    /// </summary>
    private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    /// <summary>
    /// 1.创建一个命令缓冲区并给他一个名字，方便在FrameDebug中查看，commandBuffer在命令发送后并不会清空！！！重复使用
    /// 需手动清空
    /// 2.context只能发送部分渲染命令，其余需要通过commandBuffer,至于哪些？还不知道
    /// </summary>
    private CommandBuffer _commandBuffer = new CommandBuffer()
    {
        //ToDo 了解下commandBuffer和contextBuffer的区别
        //为属性赋值。。。。这语法，我怎么知道他有什么属性。。。
        name = _commandBufferName
    };
    
    /// <summary>
    /// 摄像机渲染Pipeline
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;

        /////剔除/////////////////////
        if (!Cull())
        {
            Debug.LogError("剔除失败");
            return;
        }
        
        /////渲染Begin////////////////
        SetUpCamera();

        DrawVisibleGeometry();
        
        DrawUnsupportedShaders();
        /////渲染End///////////////////
        
        
        //////提交/////////
        SubmitRenderOrder();
        
        
    }

    /// <summary>
    /// 设置摄像机渲染的基础属性，投影矩阵啊什么的，自然是要在物体渲染之前设置好，
    /// 以及一些基本清空缓冲什么的
    /// </summary>
    private void SetUpCamera()
    {
        //这条写这是为了加快buffer清理，so why？
        _context.SetupCameraProperties(_camera);
        
        //一开始为避免下一帧图像受上一帧影响需清理下帧缓冲区
        //第三个参数是什么意思
        //这条提前是为了改一个frameDebug commandBuffer名嵌套显示的错误，因为clearRender会默认套一层
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);
        //渲染开始和结束时处理commandBuffer sampler以便在profile和Frame Debug中显示渲染流程的一些信息——————
        //只是用于调试
        _commandBuffer.BeginSample(_commandBufferName);
        
        //beginSample也属于commandBuffer的命令，要使其生效则必须手动执行
        ExecuteCommandBuffer();
    }
    
    /// <summary>
    /// 渲染可见几何物体
    /// </summary>
    private void DrawVisibleGeometry()
    {
        //DrawRenderers相关API： https://docs.unity.cn/cn/2020.3/ScriptReference/Rendering.ScriptableRenderContext.DrawRenderers.html
        /////物体绘制
        //描述对象渲染时的排序方式，传入DrawingSettings中
        SortingSettings sortingSettings = new SortingSettings(_camera)
        {
            //渲染时执行的排序类型?啥呀？
            criteria = SortingCriteria.CommonOpaque
        };
        
        DrawingSettings drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings);
        //用于过滤渲染队列里的对象
        //先渲染不透明
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        
        //绘制天空盒至摄像机
        _context.DrawSkybox(_camera);
        
        //渲染透明物体，最后，不然会被天空遮住
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);



    }

    /// <summary>
    /// context的渲染命令以缓冲区形式存在，需要手动提交缓冲区命令
    /// </summary>
    private void SubmitRenderOrder()
    {
        //提交命令前结束对渲染commandBuffer的调试sample
        _commandBuffer.EndSample(_commandBufferName);
        ExecuteCommandBuffer();
        
        _context.Submit();
    }

    /// <summary>
    /// 说是说执行命令常和清除一起，但总觉得不方便，就先这样吧______
    /// 实际上由context发起的commandBuffer命令调用并没有执行，而是将其添加到了context的buffer，
    /// 并在context的submit阶段统一执行，套娃是吧。。。。
    /// </summary>
    private void ExecuteCommandBuffer()
    {
        _context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    /// <summary>
    /// 剔除操作
    /// </summary>
    /// <returns></returns>
    private bool Cull()
    {
        //用于配置可编程渲染管线中的剔除操作的参数 https://docs.unity.cn/cn/2020.3/ScriptReference/Rendering.ScriptableCullingParameters.html
        //从摄像机获取剔除所需数据，在接下来用该数据进行正式剔除，SRP下应该可以手动改吧。。。。应该。。。
        ScriptableCullingParameters scriptableCullingParameters;

        if (_camera.TryGetCullingParameters(out scriptableCullingParameters))
        {
            _cullingResults = _context.Cull(ref scriptableCullingParameters);
            return true;
        }

        return false;
    }

}
