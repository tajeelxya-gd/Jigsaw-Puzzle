using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CannonGridEditorWindow : EditorWindow
{
    private LevelEditor _levelEditor;
    public static void OpenWindow(LevelEditor levelEditor)
    {
        var window = GetWindow<CannonGridEditorWindow>("Cannon Grid Editor");
        window._levelEditor = levelEditor;
        window.Show();
    }

    private GridData grid = new GridData(3, 10);
    private Vector2 scroll;
    private float cellSize = 40f;

    private bool isPainting = false;
    private bool changed = false;

    private Texture2D texWhite;

    private enum BrushMode { Paint, Remove, Connect }
    private BrushMode currentMode = BrushMode.Paint;

    private ColorType colorType;
    private ShooterType shooterType;
    private int ProjectileAmount = 1;
    private bool IsHidden;

    private Rect gridRect;
    private Dictionary<ColorType, Color> cachedColors = new Dictionary<ColorType, Color>();

    private CannonData selectedConnector = null;
    private CannonData selectedTarget = null;
    private int selectedConnectorX = -1;
    private int selectedConnectorY = -1;

    private void OnEnable()
    {
        texWhite = Texture2D.whiteTexture;
    }

    private void OnGUI()
    {


        if (GUILayout.Button("Save"))
        {
            ConvertDataToCannonData();
        }
        if (GUILayout.Button("Load"))
        {
            GetCannonDataAndPaint(_levelEditor.GetCannonData());
        }
        if (GUILayout.Button("Clear"))
        {
            ClearGrid();
        }
        DrawInspectorPanel();
        DrawSeparator();
        DrawGridPanel();
        DrawSeparator();
        if (changed)
        {
            Repaint();
            changed = false;
        }
    }

    private void DrawInspectorPanel()
    {
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
        GUILayout.Label("Shooter Type", EditorStyles.boldLabel);
        shooterType = (ShooterType)EditorGUILayout.EnumPopup(shooterType);
        if (EditorGUI.EndChangeCheck())
        {
            OnShooterValChange();
        }

        GUILayout.Space(5);
        GUILayout.Label("Projectile It Contains", EditorStyles.boldLabel);
        ProjectileAmount = EditorGUILayout.IntField(ProjectileAmount);

        GUILayout.Space(5);
        GUILayout.Label("Is Hidden", EditorStyles.boldLabel);
        IsHidden = EditorGUILayout.Toggle(IsHidden);

        GetRemainingEnemyCount();
        DrawRemainingEnemyInfo();
    }
    private void OnShooterValChange()
    {
        if (shooterType == ShooterType.LockedCannon)
        {
            ProjectileAmount = 0;
            colorType = ColorType.Clear;
        }
    }

    private void DrawRemainingEnemyInfo()
    {
        GUILayout.Label("Remaining Enemy Health Per Color", EditorStyles.boldLabel);

        foreach (var pair in _remainingEnemyCountAgainstCannon)
        {
            GUILayout.BeginHorizontal();
            GUI.color = GetCachedColorSafe(pair.Key);
            GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
            GUI.color = Color.white;

            GUILayout.Label($"{pair.Key}: {pair.Value} HP Remaining", GUILayout.Height(20));
            GUILayout.EndHorizontal();
        }

        GUI.color = Color.white;
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
        gridRect = GUILayoutUtility.GetRect(gridWidth, gridHeight);

        Event e = Event.current;
        Vector2 mouse = e.mousePosition;
        bool inside = gridRect.Contains(mouse);

        Vector2 localMouse = mouse - new Vector2(gridRect.x, gridRect.y);

        if ((e.rawType == EventType.MouseDown || (e.rawType == EventType.MouseDrag && isPainting)) && inside)
        {
            if (e.rawType == EventType.MouseDown)
                isPainting = true;

            int x = Mathf.FloorToInt(localMouse.x / cellSize);
            int y = Mathf.FloorToInt(localMouse.y / cellSize);
            int idx = grid.Index(x, y);

            if (currentMode == BrushMode.Paint)
                PaintCell(localMouse);
            else if (currentMode == BrushMode.Connect)
                HandleConnection(grid.data[idx], x, y);
            else
                EraseCell(localMouse);

            e.Use();
        }

        if (e.rawType == EventType.MouseUp)
            isPainting = false;


        for (int y = 0; y < grid.rows; y++)
        {
            for (int x = 0; x < grid.columns; x++)
            {
                Rect r = new Rect(
                    gridRect.x + x * cellSize,
                    gridRect.y + y * cellSize,
                    cellSize - 1,
                    cellSize - 1
                );

                var cell = grid.data[grid.Index(x, y)];

                // Background color
                if (cell == null)
                    GUI.color = new Color(0, 0, 0, 0.05f);
                else
                {
                    GUI.color = GetCachedColorSafe(cell.colorType);
                    if (cell.IsHidden)
                        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.5f);
                    if (cell.shooterType == ShooterType.LockedCannon)
                        GUI.color = Color.gold;
                }

                GUI.DrawTexture(r, texWhite);

                if (cell != null)
                {
                    GUI.color = Color.white; // text color
                    var style = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 20
                    };

                    string label = cell.ProjectileAmount.ToString();

                    if (cell.isConnected)
                    {
                        label += $"\n[C ({cell.connectedColumn},{cell.connectedRow})]";
                    }
                    if (cell.shooterType == ShooterType.LockedCannon) label = "L";

                    GUI.Label(r, label, style);
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

        GUILayoutUtility.GetRect(gridWidth, gridHeight);
        EditorGUILayout.EndScrollView();
    }

    private void PaintCell(Vector2 localMousePos)
    {
        int x = Mathf.FloorToInt(localMousePos.x / cellSize);
        int y = Mathf.FloorToInt(localMousePos.y / cellSize);

        //    y = grid.rows - 1 - y;

        if (x >= 0 && x < grid.columns && y >= 0 && y < grid.rows)
        {
            int idx = grid.Index(x, y);
            var existing = grid.data[idx];

            if (existing != null &&
                existing.colorType == colorType &&
                existing.shooterType == shooterType &&
                existing.ProjectileAmount == ProjectileAmount)
                return;

            grid.data[idx] = new CannonData
            {
                shooterType = shooterType,
                colorType = colorType,
                ProjectileAmount = ProjectileAmount,
                IsHidden = IsHidden,
                isConnected = false
            };

            changed = true;
        }
    }

    private void EraseCell(Vector2 localMousePos)
    {
        int x = Mathf.FloorToInt(localMousePos.x / cellSize);
        int y = Mathf.FloorToInt(localMousePos.y / cellSize);

        //   y = grid.rows - 1 - y;

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

    private int nextConnectionID = 1;
    private void HandleConnection(CannonData cell, int x, int y)
    {
        Debug.LogError(cell);
        if (cell == null) return;

        if (cell.isConnected) return;

        if (selectedConnector == null)
        {
            selectedConnector = cell;
            selectedConnectorX = x;
            selectedConnectorY = y;
        }
        else
        {
            // Prevent connecting to itself
            if (selectedConnector == cell)
            {
                selectedConnector = null;
                selectedConnectorX = -1;
                selectedConnectorY = -1;
                return;
            }

            selectedTarget = cell;

            if (selectedTarget.isConnected)
            {
                selectedConnector = null;
                selectedConnectorX = -1;
                selectedConnectorY = -1;
                return;
            }

            int connectionID = nextConnectionID++;

            selectedConnector.isConnected = true;
            selectedConnector.connectedColumn = x;
            selectedConnector.connectedRow = y;

            selectedTarget.isConnected = true;
            selectedTarget.connectedColumn = selectedConnectorX;
            selectedTarget.connectedRow = selectedConnectorY;

            selectedConnector = null;
            selectedTarget = null;
            selectedConnectorX = -1;
            selectedConnectorY = -1;

            changed = true;
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

    private void ConvertDataToCannonData()
    {
        var converted = ConvertGridToCannonData();
        _levelEditor.RecieveCannonData(converted);
    }
    private LevelData.CannonData[] ConvertGridToCannonData()
    {
        List<LevelData.CannonData> result = new List<LevelData.CannonData>();

        for (int x = 0; x < grid.columns; x++)
        {
            LevelData.CannonData columnData = new LevelData.CannonData();
            List<LevelData.CannonData.CannonColumn> cannonColumns = new List<LevelData.CannonData.CannonColumn>();

            for (int y = 0; y < grid.rows; y++)
            {
                int idx = grid.Index(x, y);
                var cell = grid.data[idx];

                if (cell == null)
                    continue;

                LevelData.CannonData.CannonColumn col = new LevelData.CannonData.CannonColumn
                {
                    shooterType = cell.shooterType,
                    colorType = cell.colorType,
                    ProjectileAmount = cell.ProjectileAmount,
                    isHidden = cell.IsHidden,
                    isConnected = cell.isConnected,
                    connectedColumn = cell.connectedColumn,
                    connectedRow = cell.connectedRow
                };
                cannonColumns.Add(col);
            }

            if (cannonColumns.Count > 0)
            {
                columnData.cannonColumns = cannonColumns.ToArray();
                result.Add(columnData);
            }
        }

        return result.ToArray();
    }

    private void GetCannonDataAndPaint(LevelData.CannonData[] enemyDatas)
    {
        if (enemyDatas == null || enemyDatas.Length == 0)
            return;

        int cols = enemyDatas.Length;
        int rows = 0;

        foreach (var col in enemyDatas)
        {
            if (col.cannonColumns != null)
                rows = Mathf.Max(rows, col.cannonColumns.Length);
        }

        grid.Resize(cols, rows);

        for (int i = 0; i < grid.data.Count; i++)
            grid.data[i] = null;

        for (int x = 0; x < enemyDatas.Length; x++)
        {
            var colData = enemyDatas[x];
            if (colData.cannonColumns == null)
                continue;

            for (int y = 0; y < colData.cannonColumns.Length; y++)
            {
                var enemy = colData.cannonColumns[y];
                if (enemy == null)
                    continue;

                int idx = grid.Index(x, y);
                grid.data[idx] = new CannonData
                {
                    colorType = enemy.colorType,
                    shooterType = enemy.shooterType,
                    ProjectileAmount = enemy.ProjectileAmount,
                    IsHidden = enemy.isHidden,
                    isConnected = enemy.isConnected,
                    connectedColumn = enemy.connectedColumn,
                    connectedRow = enemy.connectedRow
                };
            }
        }

        changed = true;
    }

    private void ClearGrid()
    {
        for (int i = 0; i < grid.data.Count; i++)
        {
            grid.data[i] = null;
        }
        nextConnectionID = 1;
        changed = true;
    }

    private Dictionary<ColorType, int> _remainingEnemyCountAgainstCannon =
        new Dictionary<ColorType, int>();
    private Dictionary<ColorType, int> _cannonProjectileTotals = new Dictionary<ColorType, int>();

    private void GetRemainingEnemyCount()
    {
        _remainingEnemyCountAgainstCannon.Clear();

        LevelData.EnemyData[] enemyColumns = _levelEditor.GetEnemyData();
        if (enemyColumns == null) return;

        foreach (var column in enemyColumns)
        {
            if (column.enemyColumns == null) continue;

            foreach (var enemy in column.enemyColumns)
            {
                if (enemy == null) continue;
                if (enemy.enemyType == EnemyType.Piggy) continue;
                if (enemy.enemyType == EnemyType.Box) continue;
                if (enemy.enemyType == EnemyType.Key) continue;
                if (!_remainingEnemyCountAgainstCannon.ContainsKey(enemy.colorType))
                    _remainingEnemyCountAgainstCannon[enemy.colorType] = 0;

                if (enemy.enemyType == EnemyType.Boggie)
                    _remainingEnemyCountAgainstCannon[enemy.colorType] += 2;
                else
                    _remainingEnemyCountAgainstCannon[enemy.colorType] += enemy.EnemyHealth;
            }
        }

        LevelData.BoxData[] boxColumns = _levelEditor.GetBoxDataData();
        if (boxColumns != null)
        {
            foreach (var item in boxColumns)
            {
                if (item.enemyData == null) continue;

                foreach (var enemy in item.enemyData)
                {
                    foreach (var s in enemy.Enemies)
                    {
                        if (s == null) continue;
                        if (s.enemyType == EnemyType.Piggy) continue;
                        if (s.enemyType == EnemyType.Box) continue;
                        if (s.enemyType == EnemyType.Key) continue;
                        if (!_remainingEnemyCountAgainstCannon.ContainsKey(s.colorType))
                            _remainingEnemyCountAgainstCannon[s.colorType] = 0;

                        _remainingEnemyCountAgainstCannon[s.colorType] += 1;
                    }
                }
            }
        }

        CalculateCannonProjectileTotals();

        foreach (var pair in _cannonProjectileTotals)
        {
            if (_remainingEnemyCountAgainstCannon.ContainsKey(pair.Key))
            {
                _remainingEnemyCountAgainstCannon[pair.Key] -= pair.Value;
            }
        }
    }
    private void CalculateCannonProjectileTotals()
    {
        _cannonProjectileTotals.Clear();

        for (int i = 0; i < grid.data.Count; i++)
        {
            var cell = grid.data[i];
            if (cell == null) continue;

            if (!_cannonProjectileTotals.ContainsKey(cell.colorType))
                _cannonProjectileTotals[cell.colorType] = 0;

            _cannonProjectileTotals[cell.colorType] += cell.ProjectileAmount;
        }
    }

    public class GridData
    {
        public int columns = 8;
        public int rows = 8;
        public List<CannonData> data = new List<CannonData>();

        public GridData(int cols, int rows) => Resize(cols, rows);

        public void Resize(int cols, int rows)
        {
            columns = Mathf.Max(1, cols);
            this.rows = Mathf.Max(1, rows);

            int total = columns * this.rows;
            List<CannonData> newData = new List<CannonData>(new CannonData[total]);

            int oldTotal = data.Count;
            for (int i = 0; i < Mathf.Min(total, oldTotal); i++)
                newData[i] = data[i];

            data = newData;
        }

        public int Index(int x, int y) => y * columns + x;
    }
    public class CannonData
    {
        public ShooterType shooterType;
        public ColorType colorType;
        public int ProjectileAmount;
        public bool IsHidden;
        public bool isConnected = false;
        public int connectedColumn = -1;
        public int connectedRow = -1;
    }
}