using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManiaBar : MonoBehaviour
{
    [SerializeField] private Image[] _fillBar;
    [SerializeField] private Image[] _rewardToBeGiven;
    [SerializeField] private TextMeshProUGUI[] _progressText;
    [SerializeField] public float _fillDuration = 0.5f;
    [SerializeField] private UIParticle _particle;
    [SerializeField] private PanelScaling _scaling;

    [Button]
    public void UpdateFillBar(float progress)
    {
        progress = Mathf.Clamp01(progress);

        for (int i = 0; i < _fillBar.Length; i++)
        {
            var fillBarRect = _fillBar[i].rectTransform;
            fillBarRect.DOKill();
            float fillWidth = fillBarRect.rect.width;
            fillBarRect.DOAnchorPosX((progress - 1) * fillWidth, _fillDuration).SetEase(Ease.OutCubic);
        }
    }

    public void UpdateProgressText(int current, int total)
    {
        foreach (var txt in _progressText)
            txt.text = $"{current}/{total}";
    }

    public void UpdateRewardImage(Sprite rewardImage)
    {
        _particle.Play();
        StartCoroutine(ShowSpriteWithDelay(rewardImage));
    }

    private IEnumerator ShowSpriteWithDelay(Sprite rewardImage)
    {
        _scaling.ScaleOut();
        for (int i = 0; i < _rewardToBeGiven.Length; i++)
        {
            //_rewardToBeGiven[i].SetNativeSize();
            _rewardToBeGiven[i].gameObject.SetActive(false);
            yield return new WaitForSeconds(.25f);
            _rewardToBeGiven[i].gameObject.SetActive(true);
            _rewardToBeGiven[i].sprite = rewardImage;
        }
        _scaling.ScaleIn();
    }
}