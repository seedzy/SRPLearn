using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// 该类为一个管线实例类，供customRenderPipeline使用
/// </summary>
public class CustomRenderPipeline : RenderPipeline
{
    /// <summary>
    /// 摄像机渲染实例，用于指定特定摄像机的渲染方式
    /// </summary>
    private CustomCameraRenderer _customCameraRenderer = new CustomCameraRenderer();
    
    /// <summary>
    /// 每一帧都会调用该方法进行渲染
    /// </summary>
    /// <param name="context">恕我暂不知道这东西干嘛的</param>
    /// <param name="cameras">摄像机数组，毕竟支持多摄像机？</param>
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _customCameraRenderer.Render(context, camera);
        }
    }
}
