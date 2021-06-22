using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string bufferName = "Lighting";

    private CommandBuffer _commandBuffer = new CommandBuffer()
    {
        name = bufferName
    };

    //创建两个shaderid标识，用于把数据存储到指定变量
    private static int mainLightColorId = Shader.PropertyToID("_MainLightColor");
    private static int mainLightDirection = Shader.PropertyToID("_MainLightDirection");

    public void SetUp(ScriptableRenderContext context)
    {
        //开启一个cb采样以便在analysis里查看
        _commandBuffer.BeginSample(bufferName);
        
        SetUpMainLight();
        
        _commandBuffer.EndSample(bufferName);
        
        //将cb命令添加到context，强调只是添加未执行，执行在context的submit
        context.ExecuteCommandBuffer(_commandBuffer);
        
        //cb可复用，所以清空
        _commandBuffer.Clear();
        
    }

    void SetUpMainLight()
    {
        Light light = RenderSettings.sun;
        //获取一个平行光颜色* 强度传给shader，记得颜色要转线性空间....（没想到到这就全都是线性颜色空间了）
        _commandBuffer.SetGlobalVector(mainLightColorId, light.color.linear * light.intensity);
        //记住这方向是指向光源的方向，虽然也能反过来就是了
        _commandBuffer.SetGlobalVector(mainLightDirection, -light.transform.forward);
    }
}
