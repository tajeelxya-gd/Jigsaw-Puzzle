using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class EnemyGridEditorWindow : EditorWindow
{
    private SerializedObject _so;
    private SerializedProperty _enemyIconProp;

    private LevelEditor _levelEditor;
    [SerializeField, ReadOnly] public EnemyIconData _enemyIconData;

    private GridData grid = new GridData(10, 10);
    private Vector2 scroll;
    private float cellSize = 40f;

    private bool isPainting = false;
    private bool changed = false;

    private Texture2D texWhite;

    private enum BrushMode { Paint, Remove }
    private BrushMode currentMode = BrushMode.Paint;

    private ColorType colorType;

    private int nextPropID = -1;
    private int subPropID = -1;

    private EnemyType enemyType;
    private void OnEnemyValueChange()
    {
        if (enemyType == EnemyType.Simple)
        {
            Health = 1;
            nextPropID = -1;
            subPropID = -1;
        }
        else if (enemyType == EnemyType.BulkyStickMan)
        {
            Health = 2;
            nextPropID = -1;
            subPropID = -1;
        }
        else if (enemyType == EnemyType.Piggy || enemyType == EnemyType.Key)
        {
            Health = 20;
            colorType = ColorType.Clear;
            nextPropID = -1;
            subPropID = -1;
        }
        else if (enemyType == EnemyType.Box || enemyType == EnemyType.Boggie)
        {
            Health = 1;
            if (enemyType == EnemyType.Box)
                colorType = ColorType.Clear;
            nextPropID = 0;
            if (enemyType == EnemyType.Box)
                subPropID = -1;
        }
        else if (enemyType == EnemyType.Tyre)
        {
            Health = 20;
            nextPropID = -1;
            subPropID = -1;
        }
    }

    private int Health = 1;

    private Rect gridRect;
    private Dictionary<ColorType, Color> cachedColors = new Dictionary<ColorType, Color>();

    public static void OpenWindow(LevelEditor levelEditor)
    {
        var window = GetWindow<EnemyGridEditorWindow>("Enemy Grid Editor");
        window._levelEditor = levelEditor;
        window.Show();
    }

    private void OnEnable()
    {
        texWhite = Texture2D.whiteTexture;
        if (_enemyIconData == null)
            _enemyIconData = Resources.Load<EnemyIconData>("EnemyIconData");

        _so = new SerializedObject(this);
        _enemyIconProp = _so.FindProperty("_enemyIconData");
    }

    private void OnGUI()
    {
        DrawInspectorPanel();
        DrawSeparator();
        DrawGridPanel();
        DrawSeparator();

        if (GUILayout.Button("Save"))
        {
            ConvertDataToEnemyData();
        }
        if (GUILayout.Button("Load"))
        {
            GetEnemyDataAndPaint(_levelEditor.GetEnemyData());
        }
        if (GUILayout.Button("Clear"))
        {
            ClearGrid();
        }
        if (GUILayout.Button("Duplicate Columns"))
        {
            DuplicateColumns();
        }

        if (changed)
        {
            Repaint();
            changed = false;
        }
    }

    private void DrawInspectorPanel()
    {
        EditorGUILayout.PropertyField(_enemyIconProp);
        _so.ApplyModifiedProperties();

        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int newCols = EditorGUILayout.IntField("Columns", grid.columns);
        int newRows = EditorGUILayout.IntField("Rows", grid.rows);

        if (EditorGUI.EndChangeCheck())
        {
            grid.Resize(newCols, newRows);
            changed = true;
        }

        cellSize = EditorGUILayout.Slider("Cell Size", cellSize, 20, 80);

        GUILayout.Space(5);
        GUILayout.Label("Brush Mode", EditorStyles.boldLabel);
        currentMode = (BrushMode)EditorGUILayout.EnumPopup(currentMode);

        GUILayout.Space(5);
        GUILayout.Label("Color Type", EditorStyles.boldLabel);
        colorType = (ColorType)EditorGUILayout.EnumPopup(colorType);

        GUILayout.Space(5);
        GUILayout.Label("EnemyType", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        enemyType = (EnemyType)EditorGUILayout.EnumPopup(enemyType);
        if (EditorGUI.EndChangeCheck())
        {
            OnEnemyValueChange();
        }

        GUILayout.Space(5);
        GUILayout.Label("Health Setting Use For Boss Only", EditorStyles.boldLabel);
        Health = EditorGUILayout.IntField(Health);

        GUILayout.Space(5);
        GUILayout.Label("Prop ID", EditorStyles.boldLabel);
        nextPropID = EditorGUILayout.IntField(nextPropID);

        GUILayout.Space(5);
        GUILayout.Label("Sub Prop ID", EditorStyles.boldLabel);
        subPropID = EditorGUILayout.IntField(subPropID);
    }

    private void DrawSeparator()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Slider", GUI.skin.horizontalSlider);
        GUILayout.Space(4);
    }

    private void DrawGridPanel()
    {
        GUILayout.Label("Grid", EditorStyles.boldLabel);

        float gridWidth = grid.columns * cellSize;
        float gridHeight = grid.rows * cellSize;

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(400));
        gridRect = GUILayoutUtility.GetRect(gridWidth + 25, gridHeight + 25);

        Event e = Event.current;
        Vector2 mouse = e.mousePosition;
        bool inside = gridRect.Contains(mouse);

        Vector2 localMouse = mouse - new Vector2(gridRect.x, gridRect.y);

        if ((e.rawType == EventType.MouseDown || (e.rawType == EventType.MouseDrag && isPainting)) && inside)
        {
            if (e.rawType == EventType.MouseDown)
                isPainting = true;

            if (currentMode == BrushMode.Paint)
                PaintCell(localMouse);
            else
                EraseCell(localMouse);

            e.Use();
        }

        if (e.rawType == EventType.MouseUp)
            isPainting = false;


        for (int y = 0; y < grid.rows; y++)
        {
            int drawY = grid.rows - 1 - y;
            float yOffset = gridRect.y + drawY * cellSize;

            for (int x = 0; x < grid.columns; x++)
            {
                Rect r = new Rect(
                    gridRect.x + x * cellSize,
                    yOffset,
                    cellSize - 1,
                    cellSize - 1
                );

                var cell = grid.data[grid.Index(x, y)];

                // Background color
                if (cell == null)
                    GUI.color = new Color(0, 0, 0, 0.05f);
                else
                {
                    if (cell.enemyType == EnemyType.Piggy)
                        GUI.color = Color.clear;
                    else
                        GUI.color = GetCachedColorSafe(cell.colorType);
                }

                GUI.DrawTexture(r, texWhite);

                // Icon
                if (cell != null)
                {
                    Texture2D icon = _enemyIconData != null ? _enemyIconData.GetIcon(cell.enemyType) : null;

                    if (icon != null)
                    {
                        GUI.color = Color.white;
                        float pad = cellSize * 0.1f;
                        Rect iconRect = new Rect(
                            r.x + pad,
                            r.y + pad,
                            r.width - pad * 2f,
                            r.height - pad * 2f
                        );

                        GUI.color = Color.white;
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }
                }
            }
        }

        GUI.color = Color.white;

        for (int x = 0; x < grid.columns; x++)
        {
            Rect colRect = new Rect(gridRect.x + x * cellSize, gridRect.y, cellSize - 1, 20);
            GUI.Label(colRect, (x + 1).ToString(), EditorStyles.boldLabel);
        }

        for (int y = 0; y < grid.rows; y++)
        {
            int drawY = grid.rows - 1 - y;
            Rect rowRect = new Rect(gridRect.x, gridRect.y + drawY * cellSize, 25, cellSize);
            GUI.Label(rowRect, (y + 1).ToString(), EditorStyles.boldLabel);
        }

        GUILayoutUtility.GetRect(gridWidth + 30, gridHeight + 30);
        EditorGUILayout.EndScrollView();
    }

    private void PaintCell(Vector2 localMousePos)
    {
        int x = Mathf.FloorToInt(localMousePos.x / cellSize);
        int y = Mathf.FloorToInt(localMousePos.y / cellSize);

        y = grid.rows - 1 - y;

        if (x >= 0 && x < grid.columns && y >= 0 && y < grid.rows)
        {
            int idx = grid.Index(x, y);
            var existing = grid.data[idx];

            if (existing != null &&
                existing.colorType == colorType &&
                existing.enemyType == enemyType &&
                existing.health == Health &&
                existing.propID == nextPropID &&
                existing.subPropID == subPropID)
                return;

            grid.data[idx] = new EnemyData
            {
                colorType = colorType,
                enemyType = enemyType,
                health = Health,
                propID = (enemyType == EnemyType.Box || enemyType == EnemyType.Boggie) ? nextPropID : -1,
                subPropID = enemyType == EnemyType.Boggie ? subPropID : -1
            };

            changed = true;
        }
    }

    private void EraseCell(Vector2 localMousePos)
    {
        int x = Mathf.FloorToInt(localMousePos.x / cellSize);
        int y = Mathf.FloorToInt(localMousePos.y / cellSize);

        y = grid.rows - 1 - y;

        if (x >= 0 && x < grid.columns && y >= 0 && y < grid.rows)
        {
            int idx = grid.Index(x, y);
            if (grid.data[idx] != null)
            {
                grid.data[idx] = null;
                changed = true;
            }
        }
    }

    private Color GetCachedColorSafe(ColorType type)
    {
        if (_levelEditor == null)
            return Color.gray;

        if (!cachedColors.TryGetValue(type, out var col))
        {
            col = _levelEditor.GetColor(type);
            cachedColors[type] = col;
        }

        return col;
    }

    private void ConvertDataToEnemyData()
    {
        var converted = ConvertGridToEnemyData();
        _levelEditor.RecieveEnemyData(converted);
    }
    private LevelData.EnemyData[] ConvertGridToEnemyData()
    {
        List<LevelData.EnemyData> result = new List<LevelData.EnemyData>();

        for (int x = 0; x < grid.columns; x++)
        {
            LevelData.EnemyData columnData = new LevelData.EnemyData();
            List<LevelData.EnemyData.EnemyColumn> enemyColumns = new List<LevelData.EnemyData.EnemyColumn>();

            for (int y = 0; y < grid.rows; y++)
            {
                int idx = grid.Index(x, y);
                var cell = grid.data[idx];

                if (cell == null)
                    continue;

                ColorType _tempColorType = ColorType.Clear;
                if (cell.enemyType != EnemyType.Piggy || cell.enemyType != EnemyType.Box) _tempColorType = cell.colorType;

                LevelData.EnemyData.EnemyColumn col = new LevelData.EnemyData.EnemyColumn
                {
                    colorType = _tempColorType,
                    enemyType = cell.enemyType,
                    EnemyHealth = cell.health,
                    PropID = cell.propID,
                    subPropID = cell.subPropID
                };

                enemyColumns.Add(col);
            }

            if (enemyColumns.Count > 0)
            {
                columnData.enemyColumns = enemyColumns.ToArray();
                result.Add(columnData);
            }
        }

        return result.ToArray();
    }

    private void GetEnemyDataAndPaint(LevelData.EnemyData[] enemyDatas)
    {
        if (enemyDatas == null || enemyDatas.Length == 0)
            return;

        int cols = enemyDatas.Length;
        int rows = 0;

        foreach (var col in enemyDatas)
        {
            if (col.enemyColumns != null)
                rows = Mathf.Max(rows, col.enemyColumns.Length);
        }

        grid.Resize(cols, rows);

        for (int i = 0; i < grid.data.Count; i++)
            grid.data[i] = null;

        for (int x = 0; x < enemyDatas.Length; x++)
        {
            var colData = enemyDatas[x];
            if (colData.enemyColumns == null)
                continue;

            for (int y = 0; y < colData.enemyColumns.Length; y++)
            {
                var enemy = colData.enemyColumns[y];
                if (enemy == null)
                    continue;

                int idx = grid.Index(x, y);
                grid.data[idx] = new EnemyData
                {
                    colorType = enemy.colorType,
                    enemyType = enemy.enemyType,
                    health = enemy.EnemyHealth,
                    propID = enemy.PropID,
                    subPropID = enemy.subPropID
                };
            }
        }

        changed = true;
    }

    private void DuplicateColumns()
    {
        int oldCols = grid.columns;
        int rows = grid.rows;
        int newCols = oldCols * 2;

        List<EnemyData> newData = new List<EnemyData>(new EnemyData[newCols * rows]);

        for (int x = 0; x < oldCols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                int oldIndex = grid.Index(x, y);
                var cell = grid.data[oldIndex];

                int newIndexA = y * newCols + (x * 2);
                int newIndexB = y * newCols + (x * 2 + 1);

                if (cell != null)
                {
                    // Clone for first column copy
                    newData[newIndexA] = CloneEnemy(cell);

                    // Clone for duplicated column
                    newData[newIndexB] = CloneEnemy(cell);
                }
            }
        }

        grid.columns = newCols;
        grid.data = newData;

        changed = true;
    }
    private EnemyData CloneEnemy(EnemyData src)
    {
        return new EnemyData
        {
            colorType = src.colorType,
            enemyType = src.enemyType,
            health = src.health,
            propID = src.propID,
            subPropID = src.subPropID
        };
    }
    private void ClearGrid()
    {
        for (int i = 0; i < grid.data.Count; i++)
        {
            grid.data[i] = null;
        }

        changed = true;
    }

    public class GridData
    {
        public int columns = 8;
        public int rows = 8;
        public List<EnemyData> data = new List<EnemyData>();

        public GridData(int cols, int rows) => Resize(cols, rows);

        public void Resize(int cols, int rows)
        {
            columns = Mathf.Max(1, cols);
            this.rows = Mathf.Max(1, rows);

            int total = columns * this.rows;
            List<EnemyData> newData = new List<EnemyData>(new EnemyData[total]);

            int oldTotal = data.Count;
            for (int i = 0; i < Mathf.Min(total, oldTotal); i++)
                newData[i] = data[i];

            data = newData;
        }

        public int Index(int x, int y) => y * columns + x;
    }

    public class EnemyData
    {
        public ColorType colorType;
        public EnemyType enemyType;
        public int health;
        public int propID = -1;
        public int subPropID = -1;
    }
}