using System.Collections.Generic;
using UnityEngine;

public class SpaceGrid : IGrid
{
    private List<List<GameObject>> _columns = new List<List<GameObject>>();
    private float _xSpacing = 1f;
    private float _zSpacing = 1f;
    private Vector3 _offset = Vector3.zero;
    private int _maxRow = 5;

    public void CreateColumns(int count, float distance)
    {
        _xSpacing = distance;
        _zSpacing = distance;

        for (int i = 0; i < _columns.Count; i++)
        {
            for (int j = 0; j < _columns[i].Count; j++)
            {
                if (_columns[i][j] != null)
                    GameObject.Destroy(_columns[i][j]);
            }
        }

        _columns.Clear();

        int totalRows = Mathf.CeilToInt((float)count / _maxRow);

        for (int row = 0; row < totalRows; row++)
        {
            _columns.Add(new List<GameObject>());
        }
    }

    public void AddItemInColumn<T>(T item, int index)
    {
        if (!(item is GameObject go))
        {
            return;
        }

        int rowIndex = index / _maxRow;
        int columnIndex = index % _maxRow;

        while (_columns.Count <= rowIndex)
            _columns.Add(new List<GameObject>());

        _columns[rowIndex].Add(go);

        Space space = go.GetComponent<Space>();
        if (space != null)
        {
            space.RowIndex = rowIndex;
            space.ColumnIndex = columnIndex;
        }

        int itemsInRow = _columns[rowIndex].Count;
        float startX = ((itemsInRow - 1) * _xSpacing) / 2f;

        for (int i = 0; i < itemsInRow; i++)
        {
            float xPos = startX - i * _xSpacing;
            float zPos = rowIndex * _zSpacing;
            _columns[rowIndex][i].GetComponent<Space>().UpdatePosition(new Vector3(xPos, 0, zPos) + _offset);
            _columns[rowIndex][i].transform.localPosition = new Vector3(xPos, 0, zPos) + _offset;
        }
    }

    public void SetOffset(Vector3 offset)
    {
        _offset = offset;
    }

    public void SetSpacing(float xSpacing, float zSpacing)
    {
        _xSpacing = xSpacing;
        _zSpacing = zSpacing;
    }
}