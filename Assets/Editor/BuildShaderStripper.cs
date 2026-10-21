using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace Tractor.Editor
{
    public class BuildShaderStripper : IPreprocessShaders
    {
        public int callbackOrder => 0;

        // Что оставляем в Android-сборке (белый список)
        static readonly string[] AndroidKeepPrefixes =
        {
            "UI/",
            "UI",
            "TextMeshPro/",
            "TextMeshPro",
            "Sprites/",
            "Sprites",
            "Hidden/UI",
            "Legacy Shaders/Transparent",
            "Mobile/",
        };

        // Что режем даже если оно попало под белый список (страховка)
        static readonly string[] AndroidAlwaysExcludes =
        {
            "Enviro",
            "Fog",
            "Volumetric",
        };

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return;

            // 1. Страховочный чёрный список
            for (int i = 0; i < AndroidAlwaysExcludes.Length; i++)
            {
                if (shader.name.Contains(AndroidAlwaysExcludes[i]))
                {
                    data.Clear();
                    Debug.Log($"[BuildShaderStripper] Android: вырезан (чёрный список) '{shader.name}'");
                    return;
                }
            }

            // 2. Белый список — всё, что не UI, режем
            bool keep = false;
            for (int i = 0; i < AndroidKeepPrefixes.Length; i++)
            {
                if (shader.name.StartsWith(AndroidKeepPrefixes[i]) ||
                    shader.name.Contains(AndroidKeepPrefixes[i]))
                {
                    keep = true;
                    break;
                }
            }

            if (!keep)
            {
                data.Clear();
                Debug.Log($"[BuildShaderStripper] Android: вырезан (не UI) '{shader.name}'");
            }
        }
    }
}
