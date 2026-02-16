using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Sequence = DG.Tweening.Sequence;

public class BulkPopUpEffect : MonoBehaviour
{
    public Transform endingPosition;
    public MoveCash cashPrefab;
    public Transform economyBar;
    public float InitDelay = 0;
    public float randomDifference = 5;
    public float waitInGenerate = 0.125f;
    private WaitForSeconds _generateWait;
    public CanvasGroup _hitBlinkerEffect;
    [SerializeField] private ParticleSystem hitParticleEffect;

    public int totalNumOfCoins;

    public MoveCash[] PollingArray;
    [SerializeField] private AudioClip _popSfxEffect;
    [SerializeField] private AudioClip _hitSfxEffect;
    private IEnumerator Start()
    {
        _generateWait = new WaitForSeconds(waitInGenerate);
    
        for (int i = 0; i < PollingArray.Length; i++)
        {
            PollingArray[i] = Instantiate(cashPrefab, transform);
            PollingArray[i].gameObject.SetActive(false);
             yield return null; 
        }
    }

    private Sequence economyBarPunch;
    void PlayPunchEffect()
    {
        if (economyBarPunch != null)
        {
            economyBarPunch.Kill();
        }

        economyBar.localScale = Vector3.one;

        economyBarPunch = DOTween.Sequence();
    
        economyBarPunch.Append(economyBar.DOScale(1.2f, 0.1f).SetEase(Ease.OutQuad))
            .Append(economyBar.DOScale(1.0f, 0.25f).SetEase(Ease.InQuad)).SetUpdate(true);

        // --- Visual Effects ---
        if(_hitBlinkerEffect)
            _hitBlinkerEffect.DOFade(0, 0.5f).From(1).SetUpdate(true);

        if(hitParticleEffect)
        {
            hitParticleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitParticleEffect.Play();
        }
        AudioController.PlaySFX(_hitSfxEffect);
        
    }
    public void Generate(Transform temp = null, int Coins = 1,UnityAction OnHitPlace = null,UnityAction OnCompletAnimations = null)
    {
        if (!gameObject.activeInHierarchy) return;
        if (temp == null)
            temp = endingPosition;
        if (Coins != -1)
            totalNumOfCoins = Coins;
        AudioController.PlaySFX(_popSfxEffect);
        StartCoroutine(InstentiateCoins(temp,OnHitPlace,OnCompletAnimations));
    }

    MoveCash tempCoins;
    IEnumerator InstentiateCoins(Transform temp = null,UnityAction onComplete = null,UnityAction OnCompletAnimations = null)
    {
        yield return new WaitForSecondsRealtime(InitDelay);
        float completeDelay = 0;
        for (int i = 0; i < totalNumOfCoins; i++)
        {
            yield return _generateWait;
            tempCoins = getFromPool();
            if (tempCoins == null) yield break;
            tempCoins.gameObject.SetActive(true);
            if (temp)
                tempCoins.target = temp;

            completeDelay = tempCoins.GetMoveTime();
            tempCoins.Move(new Vector3(Random.Range(-randomDifference, randomDifference), Random.Range(-randomDifference, randomDifference), 0), null,()=>
            {
                PlayPunchEffect();
                onComplete?.Invoke();
                
            });
            // tempCoins.Move(Vector3.zero, text);
        }

        yield return new WaitForSeconds(completeDelay);
        OnCompletAnimations?.Invoke();
    }

    MoveCash getFromPool()
    {
        for (int i = 0; i < PollingArray.Length; i++)
        {
            if (!PollingArray[i].gameObject.activeInHierarchy)
                return PollingArray[i];
        }
        return null;
    }
}
