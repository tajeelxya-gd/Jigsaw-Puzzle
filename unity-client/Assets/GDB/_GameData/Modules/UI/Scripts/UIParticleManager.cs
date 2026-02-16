using System;
using Coffee.UIExtensions;
using UnityEngine;

namespace FdUiParticleManager
{
    public class UIParticleManager : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] _mainMenuBGParticle;

        private void Start()
        {
            //_mainMenuBGParticle.gameObject.SetActive(true);
            for (int i = 0; i < _mainMenuBGParticle.Length; i++)
            {
                _mainMenuBGParticle[i].Play();
            }
        }
    }
}