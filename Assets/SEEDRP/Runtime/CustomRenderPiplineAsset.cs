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
    //设置是否使用批处理
    [SerializeField] private bool useGPUInstancing = true, useDynamicBatching = true, useSRPBatcher = true;

    //传入阴影设置
    [SerializeField] private ShadowSettings _shadowSettings = default;
    /// <summary>
    /// 继承方法，需返回一个管线实例
    /// </summary>
    /// <returns></returns>
    protected override RenderPipeline CreatePipeline()
    {
        CustomRenderPipeline customRenderPipeline = new CustomRenderPipeline(useGPUInstancing, useDynamicBatching, useSRPBatcher, _shadowSettings);
        
        return customRenderPipeline;
    }
}
