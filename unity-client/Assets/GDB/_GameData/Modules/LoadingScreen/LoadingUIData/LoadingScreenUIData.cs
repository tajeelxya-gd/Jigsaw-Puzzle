using UnityEngine;

[CreateAssetMenu(fileName = "LoadingScreenUIData", menuName = "Scriptable Objects/LoadingScreenUIData")]
public class LoadingScreenUIData : ScriptableObject
{
    [SerializeField] private UI[] _LoadingUiData;

    public UI GetData(LevelType levelType)
    {
        for (int i = 0; i < _LoadingUiData.Length; i++)
        {
            if (_LoadingUiData[i].levelType == levelType)
                return _LoadingUiData[i];
        }
        return null;
    }

    [System.Serializable]
    public class UI
    {
        public LevelType levelType;
        public Sprite LoadingPanelSprite, LoadingTxtSp, EmptBarSp, RaceSp;
    }
}