using System;
using System.Collections;
using UnityEngine;

public class SearchingPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject _findOpponentPanel;
    [SerializeField] private GameObject _nextPanel;
    [SerializeField] private float _searchDuration = 3f;

    private Coroutine _searchingCoroutine;

    private void OnEnable()
    {
        if (_searchingCoroutine != null)
            StopCoroutine(_searchingCoroutine);

        _searchingCoroutine = StartCoroutine(Searching());
    }

    private IEnumerator Searching()
    {
        yield return new WaitForSeconds(_searchDuration);
        OpenNextPanel();
    }

    private void OpenNextPanel()
    {
        _nextPanel.SetActive(true);
        _findOpponentPanel.SetActive(false);
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        StopCoroutine(_searchingCoroutine);
    }
}