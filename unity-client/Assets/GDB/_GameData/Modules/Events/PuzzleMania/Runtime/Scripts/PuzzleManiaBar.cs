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
    private bool[] _hasReachedMax;

    private List<float> _maxBarWidth = new List<float>();

    void Awake()
    {
 
        _hasReachedMax = new bool[_fillBar.Length];

        for (int i = 0; i < _fillBar.Length; i++)
        {
            _maxBarWidth.Add(_fillBar[i].rectTransform.sizeDelta.x);
            _hasReachedMax[i] = false;
        }
    }

    [Button]
    public void UpdateFillBar(float progress)
    {
        progress = Mathf.Clamp01(progress);

        for (int i = 0; i < _fillBar.Length; i++)
        {
            int index = i; 

            RectTransform rt = _fillBar[index].rectTransform;
            Vector2 temp = rt.sizeDelta;

            float maxWidth = _maxBarWidth[index];
            float targetWidth = maxWidth * progress;

            rt.DOKill();

            if (_hasReachedMax[index])
            {
                rt.sizeDelta = new Vector2(0, temp.y);
                _hasReachedMax[index] = false;
            }

            rt.DOSizeDelta(new Vector2(targetWidth, temp.y), _fillDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (targetWidth >= maxWidth - 0.5f)
                    {
                        _hasReachedMax[index] = true;
                    }
                });
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