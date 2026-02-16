using System;
using UnityEngine;

public class PlayerPicture : MonoBehaviour
{
    [SerializeField] private Sprite[] _pictures;

    public Sprite GetSprite(int index)
    {
        return _pictures[index];
    }
}

