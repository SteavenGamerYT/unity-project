using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadSceneButton : MonoBehaviour
{
    public string sceneName = "";

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void EnsureBuildScenesRegistered()
    {
        string[] requiredPaths = new[]
        {
            "Assets/Odex/Scenes/Menu.unity",
            "Assets/Odex/Scenes/Leavel1.unity",
            "Assets/Odex/Scenes/Leavel2.unity",
            "Assets/Odex/Scenes/Leavel3.unity",
            "Assets/Odex/Scenes/Leavel4.unity",
            "Assets/FPS/Scenes/IntroMenu.unity",
            "Assets/FPS/Scenes/SecondaryScene.unity",
            "Assets/FPS/Scenes/WinScene.unity",
            "Assets/FPS/Scenes/LoseScene.unity",
        };

        var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;
        foreach (string path in requiredPaths)
        {
            if (!existing.Exists(s => s.path == path))
            {
                existing.Add(new EditorBuildSettingsScene(path, true));
                changed = true;
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = existing.ToArray();
        }
    }
#endif

    private void Update()
    {
        if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject 
            && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit))
        {
            LoadTargetScene();
        }
    }

    public void LoadTargetScene()
    {
        string targetScene = sceneName;
        if (targetScene == "Map_v1" || targetScene == "MainScene")
        {
            targetScene = "Menu";
        }

#if UNITY_EDITOR
        Selection.activeObject = null;
#endif
        SceneManager.LoadScene(targetScene);
    }
}
