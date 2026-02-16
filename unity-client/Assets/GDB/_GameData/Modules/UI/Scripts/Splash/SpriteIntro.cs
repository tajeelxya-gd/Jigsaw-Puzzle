using System;
using UnityEngine;
using DG.Tweening;

public class SpriteIntro : MonoBehaviour
{
    [SerializeField] private Transform[] sprites;
    [SerializeField] private Vector2[] toPositions;
    [SerializeField] private Vector2[] fromPositions;
    [SerializeField] private float duration = 0.5f; // Time for each move
    [SerializeField] private float interval = 0.2f;
    [SerializeField] private float introDelay = 0.25f;// Wait time between each tween

    private Sequence seq;

    void ResetAll()
    {
        foreach (Transform t in sprites) 
        {
            if (t != null) t.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        ResetAll();
        DOVirtual.DelayedCall(introDelay,StartIntroSequence);
    }

    void StartIntroSequence()
    {

        if (seq != null) seq.Kill();
        seq = DOTween.Sequence();

        for (int i = 0; i < sprites.Length; i++)
        {
            if (i >= toPositions.Length || i >= fromPositions.Length) break;

            RectTransform t = sprites[i].GetComponent<RectTransform>();
            Vector2 startPos = fromPositions[i]; // This is usually the anchoredPosition
            Vector2 endPos = toPositions[i];

            // 1. Setup initial state
            t.anchoredPosition = startPos;
            t.gameObject.SetActive(false); // Ensure it's off to start

            // 2. Calculate the start time for this specific sprite
            float startTime = i * interval;

            // 3. Insert the "Show" callback at the specific start time
            seq.InsertCallback(startTime, () => t.gameObject.SetActive(true));

            // 4. Insert the Move animation at the same start time
            // Using Insert ensures they overlap rather than wait for each other
            seq.Insert(startTime, t.DOAnchorPos(endPos, duration).SetEase(Ease.OutExpo));
        }
    }
}