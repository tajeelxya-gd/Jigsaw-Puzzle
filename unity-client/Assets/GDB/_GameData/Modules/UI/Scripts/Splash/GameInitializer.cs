using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] RectTransform fillBarRect; // Change to RectTransform
    [SerializeField] TMP_Text versionTxt;
    [SerializeField] private ShaderVariantCollection _shaderVariantCollection;
    [SerializeField] private float minSplashTime = 5;
    [SerializeField] private int minLvlsRequireForGamePlay = 19;
    [SerializeField] private GameObject _privacyPanel;

    private float maxBarWidth;
    private const string ManuSceneName = "MainMenu";

    private string sceneToLoad = "MainMenu";
    GameData gameData;

    private void OnEnable()
    {
        // _privacyPanel.SetActive(PlayerPrefs.GetInt("PrivacyPolicy", 0) == 0);
        // PrivacyPolicyPanel.OnPolicyAcceptedEvent += Load;
    }

    private void OnDisable()
    {
        // PrivacyPolicyPanel.OnPolicyAcceptedEvent -= Load;
    }

    void Awake()
    {
        maxBarWidth = fillBarRect.sizeDelta.x;

        fillBarRect.sizeDelta = new Vector2(0, fillBarRect.sizeDelta.y);

#if !UNITY_EDITOR
        //Debug.unityLogger.logEnabled = false;
#endif
    }

    private void Load()
    {
        gameData = GlobalService.GameData;
        sceneToLoad = ManuSceneName;
        LoadGamePlay();
    }

    public void LoadGamePlay()
    {
        versionTxt.text = $"V{Application.version}";
        StartCoroutine(LoadMenu(sceneToLoad));
    }

    IEnumerator LoadMenu(string sceneName)
    {
        // AsyncOperation MenuScene = SceneManager.LoadSceneAsync(sceneName);
        // MenuScene.allowSceneActivation = false;

        fillBarRect.DOSizeDelta(new Vector2(maxBarWidth, fillBarRect.sizeDelta.y), minSplashTime)
            .SetEase(Ease.Linear);

        float timer = 0;
        while (timer < 7f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadSceneAsync(sceneName);
        // Screen.SetResolution((int)(Screen.currentResolution.width * 0.8f), (int)(Screen.currentResolution.height * 0.8f), true);

        // yield return new WaitForSeconds(1f);
        // MenuScene.allowSceneActivation = true;
    }
}