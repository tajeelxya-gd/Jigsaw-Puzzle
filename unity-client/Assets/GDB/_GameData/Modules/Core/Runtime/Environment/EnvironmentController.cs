using UnityEngine;
[CreateAssetMenu(fileName = "EnvironmentData", menuName = "Scriptable Objects/EnvironmentData")]
public class EnvironmentController : ScriptableObject
{
    [SerializeField] private EnvironmentMaterialsData[] _environmentMaterialsData;
    [SerializeField] private Material _groundMat, _environmentMat, _gateMat, _wallMat, _wallTileMat;

    public void Initialize(LevelType levelType)
    {
        SetMaterials(GetMatData(levelType));
    }
    private void SetMaterials(EnvironmentMaterialsData environmentMaterialsData)
    {
        _groundMat.color = environmentMaterialsData.GroundMat;
        _environmentMat.color = environmentMaterialsData.EnvironmentMat;
        _gateMat.color = environmentMaterialsData.GateMat;
        _wallMat.color = environmentMaterialsData.WallMat;
        _wallTileMat.color = environmentMaterialsData.WallTileMat;
    }
    public EnvironmentMaterialsData GetMatData(LevelType levelType)
    {
        for (int i = 0; i < _environmentMaterialsData.Length; i++)
        {
            if (_environmentMaterialsData[i].levelType == levelType)
                return _environmentMaterialsData[i];
        }
        return null;
    }


    [System.Serializable]
    public class EnvironmentMaterialsData
    {
        public LevelType levelType;
        public Color GroundMat;
        public Color EnvironmentMat;
        public Color GateMat;
        public Color WallMat, WallTileMat;
        public Color SpaceColor;
    }
}