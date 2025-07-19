using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

public static class SenceMenu
{
    [MenuItem("OpenScene/TestDragon %&-")]
    private static void OpenGameScene()
    {
        OpenScene("Assets/Scenes/TestDragon.unity");
    }

    [MenuItem("OpenScene/TestCar %&1")]
    private static void OpenLevelEditor()
    {
        OpenScene("Assets/Scenes/TestCar.unity");
    }
    [MenuItem("OpenScene/Main %&=")]
    private static void OpenLoadingScene()
    {
        OpenScene("Assets/Scenes/Main.unity");
    }
    //[MenuItem("OpenScene/MeshEditor %&2")]
    //private static void OpenMeshEditor()
    //{
    //    OpenScene("Assets/_GameAssets/Scenes/MeshEditor.unity");
    //}

    private static void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}