using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 创建一个Asset创建目录，创建一个管线资产
/// </summary>
[CreateAssetMenu(menuName = "Rendering/CreateCustomRenderPipeline")]
public class CustomRenderPiplineAsset : RenderPipelineAsset
{
    /// <summary>
    /// 继承方法，需返回一个管线实例
    /// </summary>
    /// <returns></returns>
    protected override RenderPipeline CreatePipeline()
    {
        CustomRenderPipeline customRenderPipeline = new CustomRenderPipeline();
        
        return customRenderPipeline;
    }
}
