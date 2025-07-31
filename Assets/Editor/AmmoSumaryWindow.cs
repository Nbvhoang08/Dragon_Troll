using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;



public class AmmoSummaryWindow : EditorWindow
{
    private Vector2 scrollPos;
    private List<AmmoEntry> ammoList = new List<AmmoEntry>();

    private int levelIndex = 1;
    private string statusMessage = "";

    private string folderPath => Path.Combine(Application.dataPath, "Export Level");
    private string databasePath => Path.Combine(folderPath, "AmmoDatabase.json");

    [MenuItem("Tools/Bus/Ammo Summary")]
    public static void OpenWindow()
    {
        GetWindow<AmmoSummaryWindow>("Ammo Summary");
    }

    private void OnGUI()
    {
        GUILayout.Label("Ammo Summary Tool", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Level Index
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level Index:", GUILayout.Width(80));
        levelIndex = EditorGUILayout.IntField(levelIndex);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Các nút chức năng
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Recalculate Ammo"))
        {
            ammoList = AmmoCounter.CalculateAmmoInSceneAndWareHouse();
            statusMessage = $"Calculated {ammoList.Count} ammo colors for Level {levelIndex}";
        }

        if (GUILayout.Button("Save/Update Level"))
        {
            SaveOrUpdateLevel();
        }

        if (GUILayout.Button("Open Folder"))
        {
            OpenFolder();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Hiển thị danh sách ammo
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

        GUILayout.Space(5);

        if (!string.IsNullOrEmpty(statusMessage))
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
    }

    private void SaveOrUpdateLevel()
    {
        if (ammoList.Count == 0)
        {
            EditorUtility.DisplayDialog("Ammo Summary", "Ammo list is empty, calculate first!", "OK");
            return;
        }

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        AmmoDatabase database = LoadDatabase();

        // Xóa data cũ nếu tồn tại
        database.levels.RemoveAll(l => l.levelIndex == levelIndex);

        // Thêm data mới
        AmmoSummaryWrapper newLevel = new AmmoSummaryWrapper
        {
            levelIndex = levelIndex,
            ammoEntries = new List<AmmoEntry>(ammoList)
        };
        database.levels.Add(newLevel);

        // Ghi file JSON
        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(databasePath, json);

        AssetDatabase.Refresh();

        statusMessage = $"✅ Level {levelIndex} saved/updated to AmmoDatabase.json";
        Debug.Log(statusMessage);
    }

    private AmmoDatabase LoadDatabase()
    {
        if (!File.Exists(databasePath))
            return new AmmoDatabase();

        string json = File.ReadAllText(databasePath);
        return JsonUtility.FromJson<AmmoDatabase>(json) ?? new AmmoDatabase();
    }

    private void OpenFolder()
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        EditorUtility.RevealInFinder(folderPath);
    }
}

