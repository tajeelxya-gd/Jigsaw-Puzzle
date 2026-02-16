using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Animator _characterAnimator;
    [SerializeField] private WeightedAnimation[] _animations;

    private void Start()
    {
        PlayRandomAnimation();
    }

    [Button]
    public void PlayAnimation(string animationName)
    {
        _characterAnimator.SetTrigger(animationName);
    }

    public void PlayRandomAnimation()
    {
        if (_animations == null || _animations.Length == 0)
        {
            return;
        }

        float totalWeight = 0f;
        foreach (var anim in _animations)
            totalWeight += anim.weight;

        float rand = Random.value * totalWeight;

        foreach (var anim in _animations)
        {
            if (rand < anim.weight)
            {
                PlayAnimation(anim.animationName);
                return;
            }
            rand -= anim.weight;
        }
    }

    public void PlayWinAnimation()
    {
        PlayAnimation("Excited");
    }

    public void PlayLoseAnimation()
    {
        PlayAnimation("Sad");
    }
}
[System.Serializable]
public class WeightedAnimation
{
    public string animationName;
    public float weight; 
}
