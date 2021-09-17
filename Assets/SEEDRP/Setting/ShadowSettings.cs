using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
    /// <summary>
    /// 最大阴影渲染距离
    /// </summary>
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

        [Header("级联数量")]
        [Range(1, 4)] public int cascadeCount;

        [Header("级联比例")] [Range(0f, 1f)] public float cascadeRatio1;
        [Range(0f, 1f)] public float cascadeRatio2, cascadeRatio3;

        public Vector3 CascadesRatios => new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
    }

    public Directional directional = new Directional()
    {
        atlasSize = TextureSize._1024,
        cascadeCount = 4,
        cascadeRatio1 = 0.1f,
        cascadeRatio2 = 0.25f,
        cascadeRatio3 = 0.5f
    };
}


