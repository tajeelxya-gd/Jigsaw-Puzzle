using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorData", menuName = "Scriptable Objects/ColorData")]
public class ColorData : ScriptableObject
{
    public ColorRefrence[] _ColorInfo;
    public Texture QuestionMarkTexture, BigCannonQuestionMarkTexture;

#if UNITY_EDITOR
    public void AddColor(ColorRefrence color)
    {
        List<ColorRefrence> colorRefrences = new List<ColorRefrence>();
        colorRefrences = _ColorInfo.ToList();
        for (int i = 0; i < colorRefrences.Count; i++)
        {
            if (colorRefrences[i].colorType != color.colorType)
            {
                colorRefrences.Add(color);
                _ColorInfo = colorRefrences.ToArray();
                break;
            }
        }
    }
#endif

    public Color GetColor(ColorType colorType)
    {
        for (int i = 0; i < _ColorInfo.Length; i++)
        {
            if (_ColorInfo[i].colorType == colorType)
                return _ColorInfo[i].color;
        }
        return Color.clear;
    }

    public Material GetMaterial(ColorType colorType, EnemyType enemyType)
    {
        for (int i = 0; i < _ColorInfo.Length; i++)
        {
            if (_ColorInfo[i].colorType == colorType)
            {
                for (int J = 0; J < _ColorInfo[i].materialData.Length; J++)
                {
                    if (_ColorInfo[i].materialData[J].enemyType == enemyType)
                    {
                        _ColorInfo[i].materialData[J].material.color = _ColorInfo[i].color;
                        return _ColorInfo[i].materialData[J].material;
                    }
                }
            }
        }
        return null;
    }

    [System.Serializable]
    public class ColorRefrence
    {
        public Color color;
        public ColorType colorType;
        public MaterialData[] materialData;
        [System.Serializable]
        public class MaterialData
        {
            public EnemyType enemyType;
            public Material material;
        }
    }
}