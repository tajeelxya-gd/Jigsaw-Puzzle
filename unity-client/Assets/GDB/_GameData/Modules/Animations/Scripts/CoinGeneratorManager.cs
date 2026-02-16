using System.Collections;
using Coffee.UIExtensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class CoinGeneratorManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _coins;
    [SerializeField] private UIParticle _particle;
    [SerializeField] private float _timer=1.25f;
    private IMovable[] _movableCoins;

    private void Awake()
    {
        _movableCoins = new IMovable[_coins.Length];
        for (int i = 0; i < _coins.Length; i++)
        {
            _movableCoins[i] = _coins[i].GetComponent<IMovable>();
        }
    }

    [Button]
    public void GenerateCoins()
    {
        StartCoroutine(ShowParticles());
    }

    private IEnumerator ShowParticles()
    {
        for (int i = 0; i < _movableCoins.Length; i++)
        {
            _coins[i].gameObject.SetActive(true);
            _movableCoins[i].Move();
            
        }
        yield return new WaitForSeconds(_timer);
        if (_particle != null)
        {
            _particle.gameObject.SetActive(true);
            _particle.Play();
        }
    }
}