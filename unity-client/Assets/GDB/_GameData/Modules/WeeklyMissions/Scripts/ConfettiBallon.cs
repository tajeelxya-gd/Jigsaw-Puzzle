using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ConfettiBallon : MonoBehaviour
{
  [SerializeField] private GameObject _confettiEffect;
  [SerializeField] private AudioClip _hitSfxEffect;
  private float _travelTime = 2;
  private Tween _moveTween;
  public void Init(float travelTime,Tween tween)
  {
    _moveTween = tween;
    travelTime = travelTime * 0.5f;
    _travelTime =  travelTime;
    _travelTime = travelTime * Random.Range(0.5f, 0.85f);
    DOVirtual.DelayedCall(_travelTime, KillTweenig);
  }

  void KillTweenig()
  {
    AudioController.PlaySFX(_hitSfxEffect);
    _confettiEffect.gameObject.SetActive(true);
    _moveTween.Kill();
    GetComponent<Image>().DOFade(0, 0.1f).From(1);
    DOVirtual.DelayedCall(2, () =>
    {
      Destroy(gameObject);
    });
  }
  

}
