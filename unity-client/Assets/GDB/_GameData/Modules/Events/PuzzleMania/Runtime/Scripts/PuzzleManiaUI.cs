using DG.Tweening;
using TMPro;
using UnityEngine;

public class PuzzleManiaUI : MonoBehaviour
{
    [SerializeField] public HolderManager[] _holders;
    [SerializeField] private TextMeshProUGUI _grandPrize;
    [SerializeField] private GameObject _puzzleManiaPanel;
    [SerializeField] private CanvasGroup _rootCanvasGroup;
    [SerializeField] private RectTransform[] allContents;

    public void SetupUI(UIData[] data, int currentMilestoneIndex)
    {
        for (int i = 0; i < _holders.Length; i++)
        {
            _holders[i].SetupHolder(data[i]._number, data[i]._RewardProgress._rewardIcon, data[i]._RewardProgress.rewardChestAmount);
            _holders[i].SetLocked(i > currentMilestoneIndex);
        }
    }

    private Sequence _openSequence;
    private Sequence _closeSequence;

    public void UpdateUI(int index)
    {
        _holders[index].ShowCollectedIcon();
        _holders[index].DimRewardIcon();
    }

    void Reset()
    {
        _rootCanvasGroup.alpha = 0;
        _rootCanvasGroup.transform.localScale = Vector3.one;
        foreach (var obj in allContents)
        {
            obj.gameObject.SetActive(false);
        }
    }

    public void OpenPuzzleManiaPanel()
    {
        Reset();
        _openSequence = DOTween.Sequence();
        _puzzleManiaPanel.gameObject.SetActive(true);
        _openSequence.Join(_rootCanvasGroup.DOFade(1, 0.2f).From(0));
        foreach (var content in allContents)
        {
            content.gameObject.SetActive(true);
            content.localScale = Vector3.zero; // ensure starting scale
            _openSequence.Join(
                content.DOScale(1f, 0.3f).From(2).SetEase(Ease.OutBack)
            );
            _openSequence.AppendInterval(0.05f);
        }
        AudioController.PlaySFX(AudioType.ButtonClick);
        DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(AudioType.PanelPop); });

    }

    public void ClosePuzzleManiaPanel()
    {
        _closeSequence = DOTween.Sequence();
        _closeSequence.Join(_rootCanvasGroup.DOFade(0, 0.3f).From(1))
            .Join(_rootCanvasGroup.transform.DOScale(1.2f, 0.3f).From(1).OnComplete(() =>
            {

            }))
            .AppendCallback(() => _puzzleManiaPanel.gameObject.SetActive(false));
        AudioController.PlaySFX(AudioType.PanelClose);
        DOVirtual.DelayedCall(0.35f, () =>
        {
            PopCommandExecutionResponder.RemoveCommand<OnBoardingMenuCommand>();
        });
        DOVirtual.DelayedCall(0.35f + Time.deltaTime, () =>
        {
            PopCommandExecutionResponder.RemoveCommand<PuzzleRushShowCommand>();
        });

    }
    public void SetGrandPrize(int prize)
    {
        _grandPrize.text = prize.ToString();
    }

    public void ResetUI()
    {
        foreach (var holder in _holders)
            holder.Reset();
    }
}