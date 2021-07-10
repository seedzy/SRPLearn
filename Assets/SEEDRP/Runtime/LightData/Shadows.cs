using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    /// <summary>
    /// 后面再看看为什么管这叫阴影图集吧
    /// </summary>
    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    
    
    /// <summary>
    /// 限制产生阴影的最大直接光照
    /// </summary>
    private const int MaxShadowedDirectionalLightCount = 2;

    /// <summary>
    /// 记录可产生阴影的直接光照，以及其在可见光序列中的序号
    /// 不记录的话，没办法知道能产生阴影的灯光是哪一个
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
    /// 记录当前可产阴影的光源数量，不从数组获取长度是为了优化吗？
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

    /// <summary>
    /// 用于渲染阴影
    /// </summary>
    public void Render()
    {
        if (_shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalLightShadow();
        }
    }

    void RenderDirectionalLightShadow()
    {
        int atlasSize = (int)_shadowSettings.directional.atlasSize;
        
        //创建一张临时的RT，参数就是些RT用到的shaderid，大小，深度缓冲用几位什么的
        //PS ： 这个shaderId还充当了该临时RT的nameID？这什么操作
        _commandBuffer.GetTemporaryRT(
            dirShadowAtlasId, 
            atlasSize, 
            atlasSize, 
            32, 
            FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap);
        
        //设置一下这张临时RT的去向，至于详细内容看下注释吧。。。。。虽然感觉一知半解的样子
        //PS ： 现在这个大概意思就是，加载的时候，直接加载就行，不需要考虑其他的，dontcare
        //保存的时候，你给我保存就完事了
        _commandBuffer.SetRenderTarget(
            dirShadowAtlasId, 
            RenderBufferLoadAction.DontCare, 
            RenderBufferStoreAction.Store);
        
        //这里清理深度缓冲，只用清这个就行，毕竟这张RT只是存个灯光空间深度信息
        //PS ： 上一步dontCare的原因就是这里已经清过了
        _commandBuffer.ClearRenderTarget(true, false, Color.clear);
        ExecuteBuffer();
    }

    /// <summary>
    /// 渲染后清理RT，顺便因为处于shadow渲染最后一步，同时也就清理一下cbuffer
    /// </summary>
    public void CleanUp()
    {
        _commandBuffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }
    
    
    public void SetUp(ScriptableRenderContext scriptableRenderContext, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _context = scriptableRenderContext;
        _cullingResults = cullingResults;
        _shadowSettings = shadowSettings;
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
        //过滤一道光源，保证存下的是能产生阴影的光源，且距离合适
        if (_shadowedDirectionalLightCount < MaxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None &&
            light.shadowStrength > 0f &&
            _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds))
        {
            _shadowedDirectionalLights[_shadowedDirectionalLightCount++] = new ShadowedDirectionalLight()
            {
                visibleLightIndex = visibleLightIndex
            };
        }
    }

}
