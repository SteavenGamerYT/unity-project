#pragma warning disable UAC0005

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class UrpSetupUtility
{
    const string k_SettingsDir = "Assets/Settings";
    const string k_RendererDataPath = "Assets/Settings/UniversalRendererData.asset";
    const string k_UrpAssetPath = "Assets/Settings/UniversalRenderPipelineAsset.asset";

    [MenuItem("Tools/Convert Project to URP (Universal RP)")]
    public static void SetupUrp()
    {
        Type urpAssetType = FindType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset");
        Type rendererDataType = FindType("UnityEngine.Rendering.Universal.UniversalRendererData") 
            ?? FindType("UnityEngine.Rendering.Universal.ForwardRendererData");

        if (urpAssetType == null || rendererDataType == null)
        {
            Debug.LogWarning("[URP Setup] Universal Render Pipeline package is not yet fully loaded or compiled. Please ensure 'com.unity.render-pipelines.universal' has finished importing in the Package Manager.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(k_SettingsDir))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        // 1. Create or load Renderer Data
        ScriptableObject rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(k_RendererDataPath);
        if (rendererData == null)
        {
            rendererData = ScriptableObject.CreateInstance(rendererDataType);
            AssetDatabase.CreateAsset(rendererData, k_RendererDataPath);
            EditorUtility.SetDirty(rendererData);
            Debug.Log($"[URP Setup] Created RendererData at {k_RendererDataPath}");
        }

        // 2. Create or load URP Asset
        RenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(k_UrpAssetPath);
        if (urpAsset == null)
        {
            MethodInfo createMethod = urpAssetType.GetMethod("Create", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { rendererDataType }, null);
            if (createMethod != null)
            {
                urpAsset = (RenderPipelineAsset)createMethod.Invoke(null, new object[] { rendererData });
            }
            else
            {
                urpAsset = (RenderPipelineAsset)ScriptableObject.CreateInstance(urpAssetType);
                FieldInfo rendererField = urpAssetType.GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rendererField != null)
                {
                    var elemType = rendererField.FieldType.GetElementType();
                    if (elemType != null)
                    {
                        var arr = Array.CreateInstance(elemType, 1);
                        arr.SetValue(rendererData, 0);
                        rendererField.SetValue(urpAsset, arr);
                    }
                }
            }

            AssetDatabase.CreateAsset(urpAsset, k_UrpAssetPath);
            EditorUtility.SetDirty(urpAsset);
            Debug.Log($"[URP Setup] Created URP Asset at {k_UrpAssetPath}");
        }

        // 3. Assign to GraphicsSettings
        SetDefaultRenderPipeline(urpAsset);

        // 4. Assign to QualitySettings across all levels
        int qualityLevels = QualitySettings.count;
        int origLevel = QualitySettings.GetQualityLevel();
        for (int i = 0; i < qualityLevels; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = urpAsset;
        }
        QualitySettings.SetQualityLevel(origLevel, false);

        // 5. Convert Materials
        int convertedCount = UpgradeMaterialsToUrp();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[URP Setup] Project successfully configured for URP! Converted {convertedCount.ToString()} materials to URP shaders.");
        EditorUtility.DisplayDialog("URP Setup Complete", 
            $"Successfully configured Universal Render Pipeline (URP)!\n\n- Created URP Asset at {k_UrpAssetPath}\n- Assigned to Graphics Settings & Quality Settings\n- Upgraded {convertedCount.ToString()} materials to URP Lit.", "OK");
    }

    static void SetDefaultRenderPipeline(RenderPipelineAsset asset)
    {
        // Try Unity 6 defaultRenderPipeline property first
        PropertyInfo defProp = typeof(GraphicsSettings).GetProperty("defaultRenderPipeline", BindingFlags.Public | BindingFlags.Static);
        if (defProp != null && defProp.CanWrite)
        {
            defProp.SetValue(null, asset);
        }
        else
        {
            PropertyInfo legacyProp = typeof(GraphicsSettings).GetProperty("renderPipelineAsset", BindingFlags.Public | BindingFlags.Static);
            if (legacyProp != null && legacyProp.CanWrite)
            {
                legacyProp.SetValue(null, asset);
            }
        }
    }

    public static int UpgradeMaterialsToUrp()
    {
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader urpParticlesUnlit = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        Shader urpTerrainLit = Shader.Find("Universal Render Pipeline/Terrain/Lit");

        if (urpLitShader == null)
        {
            Debug.LogWarning("[URP Setup] Could not find shader 'Universal Render Pipeline/Lit'.");
            return 0;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
        int converted = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null || mat.shader == null)
                continue;

            string shaderName = mat.shader.name;

            // 1. Tree Foliage / Leaves / Cutout
            if (shaderName.Contains("Tree Soft Occlusion Leaves") || shaderName.Contains("Tree") && shaderName.Contains("Leaves"))
            {
                Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                Vector2 scale = mat.HasProperty("_MainTex") ? mat.GetTextureScale("_MainTex") : Vector2.one;
                Vector2 offset = mat.HasProperty("_MainTex") ? mat.GetTextureOffset("_MainTex") : Vector2.zero;
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;

                mat.shader = urpLitShader;
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                    mat.SetTextureScale("_BaseMap", scale);
                    mat.SetTextureOffset("_BaseMap", offset);
                }
                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_AlphaClip", 1f);
                mat.SetFloat("_Cutoff", 0.33f);
                mat.SetFloat("_Cull", 0f); // Two-sided
                mat.SetFloat("_Surface", 0f);
                mat.EnableKeyword("_ALPHATEST_ON");
                if (bumpMap != null)
                {
                    mat.SetTexture("_BumpMap", bumpMap);
                    mat.EnableKeyword("_NORMALMAP");
                }
                mat.SetOverrideTag("RenderType", "TransparentCutout");
                EditorUtility.SetDirty(mat);
                converted++;
            }
            // 2. Tree Bark
            else if (shaderName.Contains("Tree Soft Occlusion Bark") || shaderName.Contains("Tree") && shaderName.Contains("Bark"))
            {
                Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                Vector2 scale = mat.HasProperty("_MainTex") ? mat.GetTextureScale("_MainTex") : Vector2.one;
                Vector2 offset = mat.HasProperty("_MainTex") ? mat.GetTextureOffset("_MainTex") : Vector2.zero;
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;

                mat.shader = urpLitShader;
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                    mat.SetTextureScale("_BaseMap", scale);
                    mat.SetTextureOffset("_BaseMap", offset);
                }
                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_Smoothness", 0.1f);
                mat.SetFloat("_Surface", 0f);
                if (bumpMap != null)
                {
                    mat.SetTexture("_BumpMap", bumpMap);
                    mat.EnableKeyword("_NORMALMAP");
                }
                mat.SetOverrideTag("RenderType", "Opaque");
                EditorUtility.SetDirty(mat);
                converted++;
            }
            // 3. Terrain
            else if (shaderName.Contains("Terrain") && urpTerrainLit != null)
            {
                mat.shader = urpTerrainLit;
                EditorUtility.SetDirty(mat);
                converted++;
            }
            // 4. Legacy Particles
            else if (shaderName.StartsWith("Particles/") || shaderName.StartsWith("Legacy Shaders/Particles/"))
            {
                if (urpParticlesUnlit != null)
                {
                    Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                    Vector2 scale = mat.HasProperty("_MainTex") ? mat.GetTextureScale("_MainTex") : Vector2.one;
                    Vector2 offset = mat.HasProperty("_MainTex") ? mat.GetTextureOffset("_MainTex") : Vector2.zero;
                    Color color = mat.HasProperty("_TintColor") ? mat.GetColor("_TintColor") : (mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white);
                    bool isAdditive = shaderName.Contains("Additive");

                    mat.shader = urpParticlesUnlit;
                    if (mainTex != null)
                    {
                        mat.SetTexture("_BaseMap", mainTex);
                        mat.SetTextureScale("_BaseMap", scale);
                        mat.SetTextureOffset("_BaseMap", offset);
                    }
                    mat.SetColor("_BaseColor", color);
                    mat.SetFloat("_Surface", 1f); // Transparent
                    mat.SetFloat("_Blend", isAdditive ? 1f : 0f);
                    mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    mat.SetFloat("_DstBlend", isAdditive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
                    mat.SetFloat("_ZWrite", 0f);
                    mat.SetFloat("_Cull", 0f);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.renderQueue = (int)RenderQueue.Transparent;
                    if (isAdditive)
                    {
                        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        mat.EnableKeyword("_BLENDMODE_ADD");
                    }
                    else
                    {
                        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        mat.EnableKeyword("_BLENDMODE_ALPHA");
                    }
                    EditorUtility.SetDirty(mat);
                    converted++;
                }
            }
            // 5. Standard / General BIRP PBR
            else if (shaderName == "Standard" || shaderName == "Standard (Specular setup)" || shaderName.StartsWith("Mobile/Diffuse", StringComparison.Ordinal) || shaderName.StartsWith("Legacy Shaders/", StringComparison.Ordinal))
            {
                // Cache properties
                Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                Vector2 mainTexScale = mat.HasProperty("_MainTex") ? mat.GetTextureScale("_MainTex") : Vector2.one;
                Vector2 mainTexOffset = mat.HasProperty("_MainTex") ? mat.GetTextureOffset("_MainTex") : Vector2.zero;
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;

                Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                float bumpScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : 1f;

                Texture metallicMap = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;
                float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
                float smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;

                Texture emissionMap = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;
                Color emissionColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

                bool isTransparent = mat.IsKeywordEnabled("_ALPHATEST_ON") || mat.IsKeywordEnabled("_ALPHABLEND_ON") || mat.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON") || mat.renderQueue >= 3000;

                // Switch shader
                mat.shader = urpLitShader;

                // Assign URP Lit properties
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                    mat.SetTextureScale("_BaseMap", mainTexScale);
                    mat.SetTextureOffset("_BaseMap", mainTexOffset);
                }
                mat.SetColor("_BaseColor", color);

                if (bumpMap != null)
                {
                    mat.SetTexture("_BumpMap", bumpMap);
                    mat.SetFloat("_BumpScale", bumpScale);
                    mat.EnableKeyword("_NORMALMAP");
                }

                if (metallicMap != null)
                {
                    mat.SetTexture("_MetallicGlossMap", metallicMap);
                    mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                }
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);

                if (emissionMap != null || emissionColor.maxColorComponent > 0.01f)
                {
                    mat.SetTexture("_EmissionMap", emissionMap);
                    mat.SetColor("_EmissionColor", emissionColor);
                    mat.EnableKeyword("_EMISSION");
                }

                if (isTransparent)
                {
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = (int)RenderQueue.Transparent;
                }
                else
                {
                    mat.SetFloat("_Surface", 0); // Opaque
                    mat.SetInt("_SrcBlend", (int)BlendMode.One);
                    mat.SetInt("_DstBlend", (int)BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = (int)RenderQueue.Geometry;
                }

                EditorUtility.SetDirty(mat);
                converted++;
            }
        }

        return converted;
    }

    static Type FindType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = assembly.GetType(fullName);
            if (t != null)
                return t;
        }
        return null;
    }
}
