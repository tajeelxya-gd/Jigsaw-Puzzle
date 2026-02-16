using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniTx.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class MiniProgressBoxesManager : MonoBehaviour
{
    [SerializeField] private Button[] _allButtons;
    [SerializeField] private RewardProgressHolder[] _rewardProgressData;
    [SerializeField] private RectTransform _infoContainer;
    [SerializeField] private RectTransform _infoContainer_Root;
    [SerializeField] private GameObject _rewardContainerUI;

    // pool of spawned UI elements
    [ReadOnly, SerializeField]
    private List<RewardContainerUI> _pool = new();
    Sequence popSequence;
    private void Awake()
    {
        for (int i = 0; i < _allButtons.Length; i++)
        {
            int index = i; // capture correctly
            _allButtons[index].onClick.AddListener(() => BoxSelected(index));
        }
    }

    private void Start()
    {
    }

    void BoxSelected(int boxId)
    {
        _infoContainer_Root.gameObject.SetActive(true);
        StopPopupAnimation();
        RewardProgressHolder data = _rewardProgressData[boxId];

        while (_pool.Count < data._rewards.Count)
        {
            var ui = Instantiate(_rewardContainerUI, _infoContainer.transform);
            ui.gameObject.SetActive(false);
            _pool.Add(ui.GetComponent<RewardContainerUI>());
            UniStatics.LogInfo("Instantieated");
        }

        for (int i = 0; i < data._rewards.Count; i++)
        {
            var ui = _pool[i];
            ui.gameObject.SetActive(true);
            ui.Init(data._rewards[i]._rewardIcon, data._rewards[i].rewardChestAmount);
        }

        for (int i = data._rewards.Count; i < _pool.Count; i++)
        {
            _pool[i].gameObject.SetActive(false);
        }

        _infoContainer_Root.position =
            _allButtons[boxId].transform.GetChild(0).position;

        PlayPopupAnimation();
        AudioController.PlaySFX(AudioType.ButtonClick);
    }


    public void PlayPopupAnimation()
    {
        if (popSequence != null && popSequence.IsActive())
        {
            popSequence.Kill();
        }

        popSequence = DOTween.Sequence();

        popSequence.Append(
            _infoContainer_Root.transform
                .DOScale(1f, 0.3f)
                .From(0f)
                .SetEase(Ease.OutBack)
        );
        popSequence.AppendInterval(1.5f);
        popSequence.AppendCallback(() =>
        {
            _infoContainer_Root.transform
                .DOScale(0f, 0.3f)
                .From(1f)
                .SetEase(Ease.OutBack);
        });

        popSequence.Play();
    }

    public void StopPopupAnimation()
    {
        if (popSequence != null && popSequence.IsActive())
        {
            popSequence.Kill();
        }
    }

    private void OnDisable()
    {
        popSequence?.Kill();
        _infoContainer_Root.gameObject.SetActive(false);
    }
}