using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Monetization.Runtime.Consent;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] RectTransform fillBarRect;
    [SerializeField] TMP_Text versionTxt;
    [SerializeField] private float minSplashTime = 5;

    private const string ManuSceneName = "MainMenu";
    private string sceneToLoad = "MainMenu";

    private void OnEnable()
    {
        Load();
        PrivacyPolicyPanel.OnPolicyAcceptedEvent += Load;
    }

    private void OnDisable()
    {
        PrivacyPolicyPanel.OnPolicyAcceptedEvent -= Load;
    }

    void Awake()
    {
        float fillWidth = fillBarRect.rect.width;
        fillBarRect.anchoredPosition = new Vector2(-fillWidth, fillBarRect.anchoredPosition.y);
    }

    private void Load()
    {
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
        fillBarRect.DOAnchorPosX(0, minSplashTime).SetEase(Ease.Linear);

        float timer = 0;
        while (timer < 7f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadSceneAsync(sceneName);
    }
}