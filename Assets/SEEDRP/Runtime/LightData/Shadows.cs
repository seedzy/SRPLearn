using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

public class Shadows
{
    /// <summary>
    /// 限制产生阴影的最大直接光照
    /// </summary>
    private const int MaxShadowedDirectionalLightCount = 4;

    private const int MaxCascades = 4;

    private static int dirShadowAtlasId        = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static int dirShadowVPMatricesId   = Shader.PropertyToID("_DirectionalShadowVPMatrices");
    private static int cascadeCoundId          = Shader.PropertyToID("_CascadeCount");
    private static int cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
    private static int cascadeDataId           = Shader.PropertyToID("CascadeData");
    private static int shadowDistanceId        = Shader.PropertyToID("_ShadowDistance");

    //每个级联阴影有一个矩阵，每个光源阴影有maxCascades个阴影级联，所以共计MaxShadowedDirectionalLightCount * MaxCascades个VP矩阵
    private static Matrix4x4[] dirShadowVPMatrices = new Matrix4x4[MaxShadowedDirectionalLightCount * MaxCascades];
    /// <summary>
    /// 记录光照包围球数据，xyz为原点坐标，w为半径。。为什么是MaxCascades个
    /// </summary>
    private static Vector4[] cascadeCullingSpheres = new Vector4[MaxCascades];
    /// <summary>
    /// 记录各级级联数据
    /// x: 该级级联包围球半径的倒数
    /// y: 该级级联的纹素大小
    /// </summary>
    private static Vector4[] cascadeData = new Vector4[MaxCascades];
    /// <summary>
    /// 记录可产生阴影的直接光照结构，以及其在可见光序列中的序号
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
        _commandBuffer.GetTemporaryRT(
            dirShadowAtlasId, 
            atlasSize, 
            atlasSize, 
            32, 
            FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap);
        
        //设置一下RT的渲染目标为RT(其实就是把RT的数据保留下来)而不是framebuffer，至于详细内容看下注释吧。。。。。虽然感觉一知半解的样子
        //PS ： 现在这个大概意思就是，加载的时候，直接加载就行，不需要考虑其他的，dontcare
        //保存的时候，你给我保存就完事了
        _commandBuffer.SetRenderTarget(
            dirShadowAtlasId, 
            RenderBufferLoadAction.DontCare, 
            RenderBufferStoreAction.Store);
        
        //这里清理深度缓冲，只用清这个就行，毕竟这张RT只是存个灯光空间深度信息
        //PS ： 上一步dontCare的原因就是这里已经清过了
        _commandBuffer.ClearRenderTarget(true, false, Color.clear);
        _commandBuffer.BeginSample(BufferName);
        ExecuteBuffer();

        //根据产生阴影的光源数量决定图块分割系数
        //光源大于1时就把阴影图集拆成四份
        //一张shadowmap最多拆成4个灯光的阴影图，每个阴影图又可以拆成4个级联，
        //所以最终最大情况下，一张shadowmap拆成16份
        //PS:分的越多质量越差
        int tiles = _shadowedDirectionalLightCount * _shadowSettings.directional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        atlasSize /= split;
        
        //遍历每一个可产生阴影的光源渲染阴影
        for (int i = 0; i < _shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalLightShadow(i, split, atlasSize);
        }
        
        _commandBuffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
        _commandBuffer.SetGlobalInt(cascadeCoundId, _shadowSettings.directional.cascadeCount);
        _commandBuffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
        _commandBuffer.SetGlobalMatrixArray(dirShadowVPMatricesId, dirShadowVPMatrices);
        _commandBuffer.SetGlobalDepthBias(0,2);

        float cascadeFade = 1f - _shadowSettings.directional.cascadeFadeDistance;
        //传倒数，避免在shader里进行除法
        //x: shadowDis, y: shadowFade, z:cascadeFade?????????
        _commandBuffer.SetGlobalVector(shadowDistanceId, new Vector4(1f / _shadowSettings.maxDistance, 1f / _shadowSettings.distanceFade, 1f / 1f - cascadeFade * cascadeFade));
        _commandBuffer.EndSample(BufferName);
        ExecuteBuffer();
    }

    /// <summary>
    /// 渲染单个光源阴影
    /// </summary>
    /// <param name="index">光源索引</param>
    /// <param name="split">图块分割系数</param>
    /// <param name="tileSize">该光源的阴影贴图在阴影图集中所占的图块大小</param>
    void RenderDirectionalLightShadow(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = _shadowedDirectionalLights[index];
        //创建阴影设置对象？？
        var shadowSetting = new ShadowDrawingSettings(_cullingResults, light.visibleLightIndex);

        int cascadeCount = _shadowSettings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = _shadowSettings.directional.CascadesRatios;

        for (int i = 0; i < cascadeCount; i++)
        {
            //阴影贴图本质也是一张深度图，它记录了从光源位置出发，到能看到的场景中距离它最近的表面位置（深度信息）。
            //但是方向光并没有一个真实位置，我们要做地是找出与光的方向匹配的视图和投影矩阵，并给我们一个裁剪空间的立方体
            //该立方体与包含光源阴影的摄影机的可见区域重叠，这些数据的获取我们不用自己去实现，
            //可以直接调用 cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives方法，它需要9个参数。
            //第1个是可见光的索引，第 2 、 3 、 4 个参数用于设置阴影级联数据，第 5个参数是阴影贴图的尺寸，
            //第 6 个参数是阴影近平面偏移，我们先忽略它．
            //最后三个参数都是输出参数，一个是视图矩阵，一个是投影矩阵，一个是shadowSplitdata 对象，它描述有关给定阴影分割（如定向级联）的剔除信息。
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, i, cascadeCount, ratios, tileSize, 0,
                out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData);
            //ShadowSplitData描述有关给定阴影分割（如定向级联）的剔除信息,如光照包围球等。https://docs.unity.cn/cn/2020.3/ScriptReference/Rendering.ShadowSplitData.html
            if (index == 0)
            {
                SetCascadeData(i, splitData.cullingSphere, tileSize);
            }
            shadowSetting.splitData = splitData;

            int tileIndex = tileOffset + i;

            dirShadowVPMatrices[tileIndex] =
                ConvertToAtlasMatrix(projMatrix * viewMatrix, SetTileViewport(tileIndex, split, tileSize), split);
            _commandBuffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
            
        
            //执行缓冲区命令并绘制阴影
            ExecuteBuffer();
            //PS:DrawShadows只渲染shader中带shadowCasterPass的物体
            _context.DrawShadows(ref shadowSetting);
        }
    }

    void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
    {
        //最坏情况下shadowmap沿着纹素对角线方向采样，所以需要乘√2
        float texelSize = 2 * cullingSphere.w / tileSize * 1.4142136f;
        //包围球半径先平方。因为计算两点间距离时的开方操作开销很大，所以直接比较未开方的结果
        cullingSphere.w *= cullingSphere.w;
        cascadeCullingSpheres[index] = cullingSphere;
        cascadeData[index] = new Vector4(
            1f / cullingSphere.w,
            texelSize);
    }
    
    /// <summary>
    /// 修改渲染视窗，类似修改摄像机的渲染大小
    /// </summary>
    /// <param name="index"></param>
    /// <param name="split"></param>
    /// <param name="tileSize"></param>
    /// <returns>返回当前index的图块UV相对于完整图块UV的偏移</returns>
    Vector2 SetTileViewport(int index, int split, int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        _commandBuffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    /// <summary>
    /// 将完整图块VP矩阵转换为正确对应当前index图块的VP矩阵
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="offset"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 matrix, Vector2 offset, float split)
    {
        //这里把z翻转以适应不同平台，z就是矩阵的第三行
        if (SystemInfo.usesReversedZBuffer)
        {
            matrix.m20 = -matrix.m20;
            matrix.m21 = -matrix.m21;
            matrix.m22 = -matrix.m22;
            matrix.m23 = -matrix.m23;
        }

        //下面这一堆是提取了一下矩阵缩放0.5倍和矩阵平移0.5两步的有效计算，如果直接用矩阵计算会有很多和0的乘法加法的无效计算
        //虽然urp也矩阵直接乘了
        float scale = 1f / split;
        matrix.m00 = (0.5f * (matrix.m00 + matrix.m30) + offset.x * matrix.m30) * scale;
        matrix.m01 = (0.5f * (matrix.m01 + matrix.m31) + offset.x * matrix.m31) * scale;
        matrix.m02 = (0.5f * (matrix.m02 + matrix.m32) + offset.x * matrix.m32) * scale;
        matrix.m03 = (0.5f * (matrix.m03 + matrix.m33) + offset.x * matrix.m33) * scale;
        matrix.m10 = (0.5f * (matrix.m10 + matrix.m30) + offset.y * matrix.m30) * scale;
        matrix.m11 = (0.5f * (matrix.m11 + matrix.m31) + offset.y * matrix.m31) * scale;
        matrix.m12 = (0.5f * (matrix.m12 + matrix.m32) + offset.y * matrix.m32) * scale;
        matrix.m13 = (0.5f * (matrix.m13 + matrix.m33) + offset.y * matrix.m33) * scale;
        matrix.m20 =  0.5f * (matrix.m20 + matrix.m30);
        matrix.m21 =  0.5f * (matrix.m21 + matrix.m31);
        matrix.m22 =  0.5f * (matrix.m22 + matrix.m32);
        matrix.m23 =  0.5f * (matrix.m23 + matrix.m33);
        
        return matrix;
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
    public Vector2 ReserveDirectionalLightShadows(Light light, int visibleLightIndex)
    {
        //过滤一道光源，保证存下的是能产生阴影的光源，且距离合适
        if (_shadowedDirectionalLightCount < MaxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None &&
            light.shadowStrength > 0f &&
            _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds))
        {
            _shadowedDirectionalLights[_shadowedDirectionalLightCount] = new ShadowedDirectionalLight()
            {
                visibleLightIndex = visibleLightIndex
            };
            return new Vector2(light.shadowStrength, _shadowSettings.directional.cascadeCount * _shadowedDirectionalLightCount++);
        }
        return  Vector2.zero;
    }

}
