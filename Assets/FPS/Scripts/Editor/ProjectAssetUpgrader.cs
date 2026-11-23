#pragma warning disable PAC0004, PAC1001, PAC2002, PAC2004

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.ProjectAuditor.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProjectAssetUpgrader
{
    static readonly HashSet<string> k_BirpExcludedAnalyzerNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "GameObjectModuleReadWriteAnalyzer",
        "ShaderSourceAnalyzer",
        "ShaderUrpRenderabilityAnalyzer",
        "MigrateToUrpComponentAnalyzer",
        "PackagesRenderPipelineAnalyzer",
        "BuiltinRenderPipelineAnalyzer",
        "LightingSettingsAnalyzer"
    };

    static Type s_AuditorWindowType;

    [InitializeOnLoadMethod]
    static void OnEditorLoad()
    {
        var auditorAssembly = typeof(ProjectAuditor).Assembly;
        s_AuditorWindowType = auditorAssembly.GetType("Unity.ProjectAuditor.Editor.UI.ProjectAuditorWindow");

        EditorApplication.delayCall += RunUpgradeIfNeeded;
        EditorApplication.update -= ScrubOpenProjectAuditorWindows;
        EditorApplication.update += ScrubOpenProjectAuditorWindows;
    }

    static void RunUpgradeIfNeeded()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RunUpgradeIfNeeded;
            return;
        }

        if (!PlayerSettings.bakeCollisionMeshes)
        {
            PlayerSettings.bakeCollisionMeshes = true;
        }

        if (GraphicsSettings.currentRenderPipeline == null)
        {
            UrpSetupUtility.SetupUrp();
        }

        ScrubOpenProjectAuditorWindows();
    }

    static bool ShouldExcludeAnalyzer(object analyzer)
    {
        if (analyzer == null)
            return false;

        string typeName = analyzer.GetType().Name;
        if (typeName == "GameObjectModuleReadWriteAnalyzer")
            return true;

        if (GraphicsSettings.currentRenderPipeline == null && k_BirpExcludedAnalyzerNames.Contains(typeName))
            return true;

        return false;
    }

    static void ScrubOpenProjectAuditorWindows()
    {
        if (s_AuditorWindowType == null)
            return;

        try
        {
            var windows = Resources.FindObjectsOfTypeAll(s_AuditorWindowType);
            if (windows == null || windows.Length == 0)
                return;

            var visited = new HashSet<object>();
            foreach (var win in windows)
            {
                if (win != null)
                    ScrubObjectGraph(win, visited, 0);
            }
        }
        catch
        {
            // Ignore reflection errors during scrubbing
        }
    }

    static void ScrubObjectGraph(object obj, HashSet<object> visited, int depth)
    {
        if (obj == null || depth > 8)
            return;

        Type type = obj.GetType();
        if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
            return;

        if (!type.IsValueType && !visited.Add(obj))
            return;

        if (obj is IEnumerable topEnumerable && !(obj is string) && !(obj is UnityEngine.Object))
        {
            foreach (var elem in topEnumerable)
            {
                if (elem != null)
                    ScrubObjectGraph(elem, visited, depth + 1);
            }
        }

        for (var t = type; t != null && t != typeof(object); t = t.BaseType)
        {
            var ns = t.Namespace ?? string.Empty;
            if (!ns.StartsWith("Unity.ProjectAuditor", StringComparison.Ordinal))
                continue;

            foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                object val;
                try
                {
                    val = field.GetValue(obj);
                }
                catch
                {
                    continue;
                }

                if (val == null)
                    continue;

                if (val is Array arr && arr.Length > 0)
                {
                    Type elemType = arr.GetType().GetElementType();
                    if (elemType != null && elemType.Name.EndsWith("Analyzer", StringComparison.Ordinal))
                    {
                        bool hasTarget = false;
                        for (int i = 0; i < arr.Length; i++)
                        {
                            if (ShouldExcludeAnalyzer(arr.GetValue(i)))
                            {
                                hasTarget = true;
                                break;
                            }
                        }

                        if (hasTarget)
                        {
                            var kept = new List<object>(arr.Length);
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var item = arr.GetValue(i);
                                if (!ShouldExcludeAnalyzer(item))
                                    kept.Add(item);
                            }

                            Array newArr = Array.CreateInstance(elemType, kept.Count);
                            for (int i = 0; i < kept.Count; i++)
                                newArr.SetValue(kept[i], i);

                            field.SetValue(obj, newArr);
                        }
                        continue;
                    }
                }

                if (val is IList list)
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var item = list[i];
                        if (ShouldExcludeAnalyzer(item))
                        {
                            list.RemoveAt(i);
                        }
                        else if (item != null)
                        {
                            ScrubObjectGraph(item, visited, depth + 1);
                        }
                    }
                }
                else if (val is IEnumerable enumerable && !(val is string))
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null)
                            ScrubObjectGraph(item, visited, depth + 1);
                    }
                }
                else
                {
                    ScrubObjectGraph(val, visited, depth + 1);
                }
            }
        }
    }
}
