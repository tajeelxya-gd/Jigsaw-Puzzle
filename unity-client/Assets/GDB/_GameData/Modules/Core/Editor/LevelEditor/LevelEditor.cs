using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class LevelEditor : OdinEditorWindow
{
    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<LevelEditor>("Level Editor");
        window.Show();
    }

    [Title("Refrences")]
    [ReadOnly, SerializeField] public ColorData colorData;

    [Button(ButtonSizes.Small)]
    public void GetRefrences()
    {
        colorData = Resources.Load<ColorData>("ColorData");
    }

    [Title("Level Settings")]
    [Range(1, 5)]
    public int AvailableStartingSpaces = 5;

    [Range(1, 1000)]
    public int CoinRewardAmount = 10;

    [Range(0, 10)]
    public float ColumnDelayTime = 1;

    public bool HasGoal;
    [ShowIf("@HasGoal == true")] public int GoalAmount;

    [Title("Level Type")]
    public LevelType levelType;

    [BoxGroup("Color Adding")]
    [GUIColor("Green")]
    [Button(ButtonSizes.Gigantic)]
    public void AddColorEnum(string ColorName)
    {
        string safeName = EnumEditorUtility.SanitizeEnumName(ColorName);

        string path = "Assets/_GameData/Modules/Core/Runtime/GlobalEnums/ColorType.cs";
        EnumEditorUtility.AddEnumValue(path, "ColorType", safeName);
    }
    [BoxGroup("Color Adding")]
    [GUIColor("Green")]
    [Button(ButtonSizes.Gigantic)]
    public void AsignColorToEnum(Color color, ColorType colorType)
    {
        colorData.AddColor(new ColorData.ColorRefrence
        {
            color = color,
            colorType = colorType
        });
        EditorUtility.SetDirty(colorData);
        AssetDatabase.SaveAssets();
    }
    [BoxGroup("Editor")]
    [GUIColor("Orange")]
    [Button(ButtonSizes.Gigantic)]
    public void Clear()
    {
        _cannonData = null;
        _enemyData = null;
        _boxData = null;
        _boggieData = null;
    }
    [BoxGroup("Editor")]
    [GUIColor("Orange")]
    [Button(ButtonSizes.Gigantic)]
    public void OpenEnemyGridEditor()
    {
        EnemyGridEditorWindow.OpenWindow(this);
    }

    [BoxGroup("Editor")]
    [GUIColor("Yellow")]
    [Button(ButtonSizes.Gigantic)]
    public void OpenCannonGridEditor()
    {
        CannonGridEditorWindow.OpenWindow(this);
    }

    [BoxGroup("Editor")]
    [GUIColor("Blue")]
    [Button(ButtonSizes.Gigantic)]
    public void OpenBoxGridEditor()
    {
        BoxGridEditorWindow.OpenWindow(this);
    }

    [BoxGroup("Functions")]
    [GUIColor("Green")]
    [Button(ButtonSizes.Gigantic)]
    public void SaveToJson()
    {
        string folderPath = "Assets/Resources/Levels";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        int levelNumber = 1;
        string[] existing = Directory.GetFiles(folderPath, "Level *.json");
        foreach (var file in existing)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string[] parts = name.Split(' ');

            if (parts.Length == 2 && int.TryParse(parts[1], out int num))
                if (num >= levelNumber) levelNumber = num + 1;
        }

        LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
        levelData.AvailableStartingSpaces = AvailableStartingSpaces;
        levelData.CoinRewardAmount = CoinRewardAmount;
        levelData.ColumnDelayTime = ColumnDelayTime;
        levelData.levelType = levelType;
        levelData.HasGoal = HasGoal;
        levelData.GoalAmount = GoalAmount;

        levelData.cannonData = _cannonData;
        levelData.enemyData = _enemyData;
        levelData.boxData = _boxData;
        levelData.boggiesData = _boggieData;

        string json = JsonUtility.ToJson(levelData, true);

        string filePath = Path.Combine(folderPath, $"Level {levelNumber}.json");

        File.WriteAllText(filePath, json);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    [GUIColor("Blue")]
    [Button(ButtonSizes.Gigantic)]
    public void LoadFromJson(TextAsset textFile)
    {
        if (textFile == null)
            return;

        LevelData temp = ScriptableObject.CreateInstance<LevelData>();
        JsonUtility.FromJsonOverwrite(textFile.text, temp);

        AvailableStartingSpaces = temp.AvailableStartingSpaces;
        CoinRewardAmount = temp.CoinRewardAmount;
        ColumnDelayTime = temp.ColumnDelayTime;
        levelType = temp.levelType;
        _enemyData = temp.enemyData;
        _cannonData = temp.cannonData;
        _boxData = temp.boxData;
        _boggieData = temp.boggiesData;
        HasGoal = temp.HasGoal;
        GoalAmount = temp.GoalAmount;
    }
    [GUIColor("Yellow")]
    [Button(ButtonSizes.Gigantic)]
    public void OverWriteJson(TextAsset textFile)
    {
        if (textFile == null)
            return;

        string assetPath = AssetDatabase.GetAssetPath(textFile);
        if (string.IsNullOrEmpty(assetPath))
            return;

        string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath);

        LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
        levelData.AvailableStartingSpaces = AvailableStartingSpaces;
        levelData.CoinRewardAmount = CoinRewardAmount;
        levelData.ColumnDelayTime = ColumnDelayTime;
        levelData.levelType = levelType;
        levelData.HasGoal = HasGoal;
        levelData.GoalAmount = GoalAmount;

        levelData.cannonData = _cannonData;
        levelData.enemyData = _enemyData;
        levelData.boxData = _boxData;
        levelData.boggiesData = _boggieData;

        string json = JsonUtility.ToJson(levelData, true);

        File.WriteAllText(fullPath, json);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    [ReadOnly, SerializeField] private LevelData.EnemyData[] _enemyData;

    public void RecieveEnemyData(LevelData.EnemyData[] enemyData)
    {
        Debug.LogError(enemyData);
        _enemyData = enemyData;
    }
    public LevelData.EnemyData[] GetEnemyData()
    {
        return _enemyData;
    }

    [ReadOnly, SerializeField] private LevelData.CannonData[] _cannonData;

    public void RecieveCannonData(LevelData.CannonData[] cannonDatas)
    {
        _cannonData = cannonDatas;
    }
    public LevelData.CannonData[] GetCannonData()
    {
        return _cannonData;
    }

    public Color GetColor(ColorType colorType)
    {
        if (colorData == null)
            colorData = Resources.Load<ColorData>("ColorData");

        return colorData.GetColor(colorType);
    }

    [ReadOnly, SerializeField] private LevelData.BoxData[] _boxData;

    [BoxGroup("Functions")]
    [Button(ButtonSizes.Gigantic)]
    public void ConvertToBoxData()
    {
        List<LevelData.BoxData> boxDatas = new List<LevelData.BoxData>();
        for (int i = 0; i < _enemyData.Length; i++)
        {
            for (int J = 0; J < _enemyData[i].enemyColumns.Length; J++)
            {
                if (_enemyData[i].enemyColumns[J].enemyType == EnemyType.Box)
                {
                    LevelData.BoxData data = new LevelData.BoxData { PropID = _enemyData[i].enemyColumns[J].PropID, ColumnID = i };
                    bool contain = false;
                    for (int L = 0; L < boxDatas.Count; L++)
                    {
                        if (boxDatas[L].PropID == data.PropID)
                        {
                            contain = true;
                            boxDatas[L].SecondColumnID = i;
                        }
                    }
                    if (!contain)
                    {
                        boxDatas.Add(data);
                    }
                }
            }
        }
        _boxData = boxDatas.ToArray();
    }

    public void RecieveBoxData(LevelData.BoxData.EnemyInfo[] boxData, int Index)
    {
        if (Index < 0 || Index >= _boxData.Length)
        {
            Debug.LogError($"Invalid BoxData index: {Index}");
            return;
        }
        _boxData[Index].enemyData = boxData;
    }

    public LevelData.BoxData.EnemyInfo[] GetBoxDataData(int Index)
    {
        return _boxData[Index].enemyData;
    }
    public LevelData.BoxData[] GetBoxDataData()
    {
        return _boxData;
    }

    [ReadOnly, SerializeField] private LevelData.BoggieData[] _boggieData;
    [BoxGroup("Functions")]
    [Button(ButtonSizes.Gigantic)]
    public void ConvertToBoggieData()
    {
        Dictionary<int, List<LevelData.EnemyData.EnemyColumn>> boggieGroups = new();

        foreach (var enemyData in _enemyData)
        {
            if (enemyData?.enemyColumns == null)
                continue;

            foreach (var column in enemyData.enemyColumns)
            {
                if (column.enemyType != EnemyType.Boggie)
                    continue;

                if (!boggieGroups.TryGetValue(column.PropID, out var list))
                {
                    list = new List<LevelData.EnemyData.EnemyColumn>();
                    boggieGroups.Add(column.PropID, list);
                }

                list.Add(column);
            }
        }

        List<LevelData.BoggieData> boggieDataList = new();

        foreach (var group in boggieGroups)
        {
            int propID = group.Key;
            int count = group.Value.Count;

            LevelData.BoggieData boggieData = new LevelData.BoggieData
            {
                BoggieID = propID,
                colorType = group.Value[0].colorType,
                BoggieParts = new LevelData.BoggieData.Boggie[count],
                MaxLastID = count - 1
            };

            for (int i = 0; i < count; i++)
            {
                boggieData.BoggieParts[i] = new LevelData.BoggieData.Boggie
                {
                    ID = i
                };
            }

            boggieDataList.Add(boggieData);
        }

        _boggieData = boggieDataList.ToArray();
    }
    public LevelData.BoggieData[] GetBoggieDataData()
    {
        return _boggieData;
    }
}