using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LevelsGraphicsConfigurationUtility : MonoBehaviour
{
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    [Title("Serialized-References")]
    [SerializeField] private LevelsGraphicsUtilityModelUI graphicsUI;
    [Title("Data")]
    [SerializeField] private LevelGraphicsData []graphicsData;

    private void Awake()
    {
     SignalBus.Subscribe<OnGraphicsUtilityActivatedSignal>(OnGraphicsUpdateSignal);   
    }

    LevelGraphicsData GetConfigData(LevelType levelType)
    {
        foreach (var data in graphicsData)
        {
            if(data.LevelType == levelType)
                return data;
        }

        return null;
    }
    

    void OnGraphicsUpdateSignal(OnGraphicsUtilityActivatedSignal signal)
    {
        graphicsUI.Inject(GetConfigData(signal.LvlType));
    }


    [System.Serializable]
    private class LevelsGraphicsUtilityModelUI
    {
        [SerializeField] private Image bgImage;
        [SerializeField] private Image bgImageEnhanced;
        [SerializeField] private MeshRenderer towerFrontMaterial;
        [SerializeField] private DifficultyLevelDependents []difficultyLevelDependents;
        public void Inject(LevelGraphicsData graphicsData) => SetUpGraphicsData(graphicsData);

        void SetUpGraphicsData(LevelGraphicsData graphicsData)
        {
            bgImage.sprite = graphicsData.BgSprite;
            bgImageEnhanced.sprite = graphicsData.BgSprite;
            towerFrontMaterial.materials[0].SetTexture(MainTex,graphicsData.TowerSpriteBackground);
            towerFrontMaterial.materials[1].SetTexture(MainTex,graphicsData.TowerSprite);
            foreach (var levelDep in difficultyLevelDependents)
            {
                levelDep.ActivateObjects(levelDep.LevelType == graphicsData.LevelType);
            }
        }
        
    }

    [Serializable]
    private class DifficultyLevelDependents
    {
        [SerializeField] LevelType levelType;
        public LevelType  LevelType => levelType;
        [SerializeField] private GameObject[] dependentObject;

        public void ActivateObjects(bool status)
        {
            foreach (var obj in dependentObject) obj.gameObject.SetActive(status);
        }
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnGraphicsUtilityActivatedSignal>(OnGraphicsUpdateSignal);   

    }
}

public class OnGraphicsUtilityActivatedSignal : ISignal{public LevelType LvlType;}



