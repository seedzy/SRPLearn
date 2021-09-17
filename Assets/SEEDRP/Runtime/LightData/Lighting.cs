using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Lighting
{
    /// <summary>
    /// 存储经过unity剔除过后的数据
    /// </summary>
    private CullingResults _cullingResults;

    /// <summary>
    /// URP里是一个主光源和其他附加光源，这里自己写就不管了，直接全部改方向光了
    /// </summary>
    private const int MaxDirLightCount = 4;
    
    
    private const string BufferName = "Lighting";

    private CommandBuffer _commandBuffer = new CommandBuffer()
    {
        name = BufferName
    };

    //创建两个shaderid标识，用于把数据存储到指定变量
    private static int _dirLightCountId      = Shader.PropertyToID("_DirectionalLightCount");
    private static int _dirLightColorId      = Shader.PropertyToID("_DirectionalLightColor");
    private static int _dirLightDirectionId  = Shader.PropertyToID("_DirectionalLightDirection");
    private static int _dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");
    
    //存储可见关数据
    private Vector4[] _dirLightColors     = new Vector4[MaxDirLightCount];
    private Vector4[] _dirLightDirections = new Vector4[MaxDirLightCount];
    private Vector4[] _dirLightShadowData = new Vector4[MaxDirLightCount];

    /// <summary>
    /// lighting里的shadows
    /// </summary>
    private Shadows _shadows = new Shadows();

    public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _cullingResults = cullingResults;
        //开启一个cb采样以便在analysis里查看
        _commandBuffer.BeginSample(BufferName);
        
        //进行阴影设置
        _shadows.SetUp(context, cullingResults, shadowSettings);
        
        SetUpLight();
        //阴影RT渲染应放到SetUpLight()对每个光源进行阴影生成过滤之后
        _shadows.Render();

        
        _commandBuffer.EndSample(BufferName);
        
        //将cb命令添加到context，强调只是添加未执行，执行在context的submit
        context.ExecuteCommandBuffer(_commandBuffer);
        
        //cb可复用，所以清空。
        _commandBuffer.Clear();
        
    }

    /// <summary>
    /// 从unity读取可见光数据
    /// </summary>
    /// <param name="index"></param>
    /// <param name="visibleLight"></param>
    void GetDirLightData(int index,ref VisibleLight visibleLight)
    {
        _dirLightColors[index] = visibleLight.finalColor;
        //这里转换矩阵构建就不说了，参考TBN矩阵构建，带一句，是按照XZY轴的顺序构建的，所以正方向取Z轴就是第二列
        _dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        //为该可见光存储阴影数据，如果满足条件的话
        _dirLightShadowData[index] =_shadows.ReserveDirectionalLightShadows(visibleLight.light, index);
    }
    
    /// <summary>
    /// 获取并发送多个光源数据
    /// </summary>
    void SetUpLight()
    {
        // Light light = RenderSettings.sun;
        // //获取一个平行光颜色* 强度传给shader，记得颜色要转线性空间....（没想到到这就全都是线性颜色空间了）
        // _commandBuffer.SetGlobalVector(mainLightColorId, light.color.linear * light.intensity);
        // //记住这方向是指向光源的方向，虽然也能反过来就是了
        // _commandBuffer.SetGlobalVector(mainLightDirection, -light.transform.forward);

        //可见光剔除，有多重要不用多说了吧
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;

        int dirLightCount = 0;
        //循环获取平行光数据
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];

            if (visibleLight.lightType == LightType.Directional)
            {
                GetDirLightData(dirLightCount++, ref visibleLight);
            }
            //只支持4个平行光
            if (dirLightCount >= MaxDirLightCount)
                break;
        }
        //这里传了多个光照数据过去，会在Light.hlsl中继续处理
        _commandBuffer.SetGlobalInt(_dirLightCountId, dirLightCount);
        _commandBuffer.SetGlobalVectorArray(_dirLightColorId, _dirLightColors);
        _commandBuffer.SetGlobalVectorArray(_dirLightDirectionId, _dirLightDirections);
        _commandBuffer.SetGlobalVectorArray(_dirLightShadowDataId, _dirLightShadowData);
        
    }
    
    
    public void CleanUp()
    {
        _shadows.CleanUp();
    }
}
