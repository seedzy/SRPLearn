using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    private bool showPresets;
    
    /// <summary>
    /// 显示和编辑材质属性
    /// </summary>
    private MaterialEditor _materialEditor;
    /// <summary>
    /// 正在编辑的材质的引用对象
    /// </summary>
    private Object[] _materials;

    /// <summary>
    /// 可以编辑的属性的数组
    /// </summary>
    private MaterialProperty[] _materialProperties;

    /// <summary>
    /// 重载OnGUI来拓展材质编辑器
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        _materialEditor = materialEditor;
        _materials = materialEditor.targets;
        _materialProperties = properties;
        
        EditorGUILayout.Space();

        showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
        if (showPresets)
        {
            OpaquePreset();
        }
    }

    /// <summary>
    /// 设置属性值, 就是shader里的property
    /// </summary>
    /// <param name="name">属性名称</param>
    /// <param name="value">属性值</param>
    bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, _materialProperties);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }

        return false;
    }


    /// <summary>
    /// 设置关键字
    /// </summary>
    /// <param name="keyWord">关键字名称</param>
    /// <param name="enable">是否启用</param>
    void SetKeyWord(string keyWord, bool enable)
    {
        if (enable)
        {
            foreach (Material material in _materials)
            {
                material.EnableKeyword(keyWord);
            }
        }
        else
        {
            foreach (Material material in _materials)
            {
                material.DisableKeyword(keyWord);
            }
        }
    }

    /// <summary>
    /// ????????
    /// </summary>
    /// <param name="name"></param>
    /// <param name="keyword"></param>
    /// <param name="value"></param>
    void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyWord(keyword, value);
        }
    }


    private bool Clipping
    {
        set => SetProperty("_AlphaClip", "_CLIPPING", value);
    }

    private bool PremultiplyAlpha
    {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    private BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float) value);
    }
    
    private BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float) value);
    }

    private bool Zwrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

    RenderQueue RenderQueue
    {
        set
        {
            foreach (Material material in _materials)
            {
                material.renderQueue = (int) value;
            }
        }
    }

    bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            _materialEditor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            Zwrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }
}
