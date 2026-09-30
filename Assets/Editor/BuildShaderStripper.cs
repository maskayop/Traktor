using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

public class BuildShaderStripper : IPreprocessShaders
{
    public int callbackOrder => 0;

    static readonly string[] AndroidExcludes =
    {
        "Enviro",
        "Fog",
        "Volumetric"
    };

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            return;

        for (int i = 0; i < AndroidExcludes.Length; i++)
        {
            if (shader.name.Contains(AndroidExcludes[i]))
            {
                data.Clear();
                Debug.Log($"[BuildShaderStripper] Шейдер '{shader.name}' исключён из Android-сборки.");
                break;
            }
        }
    }
}
