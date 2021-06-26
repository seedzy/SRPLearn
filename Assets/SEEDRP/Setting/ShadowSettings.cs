using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
    [Min(0f)] 
    public float maxDistance = 100;
    
    public enum TextureSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }
    
    /// <summary>
    /// 设置一个光源结构，以便分别设置每种光源的阴影
    /// </summary>
    [System.Serializable]
    public struct Directional
    {
        public TextureSize atlasSize;
    }

    public Directional directional = new Directional()
    {
        atlasSize = TextureSize._1024
    };
}


