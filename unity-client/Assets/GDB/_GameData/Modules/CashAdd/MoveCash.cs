using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MoveCash : MonoBehaviour
{
    [SerializeField] private Image _image;
    public float speed = 10;
    [HideInInspector] public Transform target;
    public bool IsWordPosition;
    public float Delay;
    public AnimationCurve behavior;
    public RectTransform RT;

    public void Move(Vector3 pos, Sprite icon = null, UnityAction onReachAction = null)
    {
        RT.localPosition = pos;
        RT.localScale = Vector3.zero;
        _image.DOFade(1, 0.2f).SetUpdate(true);
        _image.sprite = icon ?? _image.sprite;
        RT.DOScale(1, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            StartMoving(onReachAction);
        }).SetUpdate(true);
    }

    public float GetMoveTime()
    {
        return Delay + (1 / speed);
    }

    private void StartMoving(UnityAction onReachAction = null)
    {
        if (target == null) return;

        Vector3 initPos = RT.position;
        Vector3 targetPos = target.GetComponent<RectTransform>().position;

        Sequence moveSequence = DOTween.Sequence();
        moveSequence.AppendInterval(Delay).SetUpdate(true)
            .Append(RT.DOMove(targetPos, 1 / speed).SetEase(Ease.InCirc).OnComplete(() =>
            {
                onReachAction?.Invoke();
            })).SetUpdate(true)
            .Append(_image.DOFade(0, 0.3f).SetDelay(0f)).SetUpdate(true)
            .Join(_image.transform.DOScale(0, 0.3f).SetDelay(0)).SetUpdate(true)
            .Join(RT.DOScale(Vector3.zero, 1f).SetDelay(0.5f)).OnComplete(() => { gameObject.SetActive(false); }).SetUpdate(true);

    }

    private void OnDisable()
    {
        gameObject.SetActive(false);
    }
}
