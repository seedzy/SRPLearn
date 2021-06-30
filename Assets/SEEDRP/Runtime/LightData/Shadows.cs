using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    /// <summary>
    /// 限制产生阴影的最大直接光照
    /// </summary>
    private const int MaxShadowedDirectionalLightCount = 2;

    /// <summary>
    /// 记录可产生阴影的直接光照，以及其在可见光序列中的序号
    /// 不记录的话，没办法知道，能产生阴影的灯光是哪一个
    /// </summary>
    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    /// <summary>
    /// 记录可产阴影的光源
    /// </summary>
    private ShadowedDirectionalLight[] _shadowedDirectionalLights =
        new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

    /// <summary>
    /// 记录已经存储的可产阴影的光源数量，不从数组获取长度是为了优化吗？
    /// </summary>
    private int _shadowedDirectionalLightCount;
    
    private const string BufferName = "Shadows";

    private CommandBuffer _commandBuffer = new CommandBuffer()
    {
        name = BufferName
    };

    //不写了，和light一样向管线中设置一些基础阴影数据
    private ScriptableRenderContext _context;

    private CullingResults _cullingResults;

    private ShadowSettings _shadowSettings;

    public void SetUp(ScriptableRenderContext scriptableRenderContext, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _context = scriptableRenderContext;
        _cullingResults = cullingResults;
        _shadowSettings = shadowSettings;

        //重置数量
        _shadowedDirectionalLightCount = 0;
    }

    void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    /// <summary>
    /// reserve(保留)，保留光源产生的阴影数据
    /// </summary>
    /// <param name="light"></param>
    /// <param name="visibleLightIndex"></param>
    public void ReserveDirectionalLightShadows(Light light, int visibleLightIndex)
    {
        // if(_shadowedDirectionalLightCount < MaxShadowedDirectionalLightCount && 
        //    light.shadows != LightShadows.None &&
        //    light.shadowStrength > 0f)
    }

}
