using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class AmmoSummaryWrapper
{
    public List<AmmoEntry> ammoEntries;
}

public class AmmoSummaryWindow : EditorWindow
{
    private Vector2 scrollPos;
    private List<AmmoEntry> ammoList = new List<AmmoEntry>();

    // Tên file mặc định
    private string fileName = "AmmoSummary";

    [MenuItem("Tools/Bus/Ammo Summary")]
    public static void OpenWindow()
    {
        GetWindow<AmmoSummaryWindow>("Ammo Summary");
    }

    private void OnGUI()
    {
        GUILayout.Label("Ammo Summary Tool", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Ô nhập tên file
        GUILayout.BeginHorizontal();
        GUILayout.Label("File Name:", GUILayout.Width(70));
        fileName = GUILayout.TextField(fileName);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Recalculate Ammo"))
        {
            ammoList = AmmoCounter.CalculateAmmoInSceneAndWareHouse();
        }

        if (GUILayout.Button("Export JSON"))
        {
            ExportToJsonInProject();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        if (ammoList.Count == 0)
        {
            GUILayout.Label("No ammo calculated yet. Click Recalculate Ammo.", EditorStyles.helpBox);
        }
        else
        {
            foreach (var entry in ammoList)
            {
                GUILayout.Label($"{entry.color}: {entry.count} bullets", EditorStyles.boldLabel);
            }
        }
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Xuất JSON vào folder Assets/Export Level/
    /// </summary>
    private void ExportToJsonInProject()
    {
        if (ammoList.Count == 0)
        {
            EditorUtility.DisplayDialog("Ammo Summary", "Ammo list is empty, calculate first!", "OK");
            return;
        }

        // Đảm bảo folder Export Level tồn tại
        string exportFolder = Path.Combine(Application.dataPath, "Export Level");
        if (!Directory.Exists(exportFolder))
            Directory.CreateDirectory(exportFolder);

        // Ghép đường dẫn file
        string safeFileName = string.IsNullOrEmpty(fileName) ? "AmmoSummary" : fileName;
        string fullPath = Path.Combine(exportFolder, safeFileName + ".json");

        // Serialize JSON
        AmmoSummaryWrapper wrapper = new AmmoSummaryWrapper { ammoEntries = ammoList };
        string json = JsonUtility.ToJson(wrapper, true);

        File.WriteAllText(fullPath, json);

        // Refresh Unity để nhận diện file
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Export Complete", $"Saved to:\nAssets/Export Level/{safeFileName}.json", "OK");
    }
}
