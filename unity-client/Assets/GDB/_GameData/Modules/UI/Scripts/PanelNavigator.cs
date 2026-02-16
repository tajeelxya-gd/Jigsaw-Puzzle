using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework.Constraints;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class PanelNavigator : MonoBehaviour
{
    [SerializeField] private List<SimpleMovement> _pages = new List<SimpleMovement>();
    [SerializeField] private bool alwaysEnabled = false;
    [ReadOnly,SerializeField]
    private int _centrePageIndex = 1;

    private void Awake()
    {
        _centrePageIndex = _pages.Count / 2;
    }



    private void Start()
    {
       SignalBus.Subscribe<OnDemandHomeScreenPanel>(OnDemandPageOpen);
    }

    void OnDemandPageOpen(OnDemandHomeScreenPanel signal)
    {
        GoToPage(signal.HomePageToOpen, signal.OnCompleteAction);
    }

    public void GoToPage(int pageIndex,UnityAction callBack = null)
    {
        if (pageIndex < 0 || pageIndex >= _pages.Count)
        {
            return;
        }

        for (int i = 0; i < _pages.Count; i++)
        {
            if (i == pageIndex)
                _pages[i].Move(callBack);
            else{
                if(i == _centrePageIndex)
                {
                    Transform _targetTransform = _pages[pageIndex > _centrePageIndex
                        ? _centrePageIndex - 1 : 
                        _centrePageIndex + 1]
                        .transform;
                    
                   // Debug.LogError("going in place of :: "+ _targetTransform);
                    _pages[_centrePageIndex].Move(null,_targetTransform);
                }else
                    _pages[i].ResetMove();
            }
        }
    }
    
    public void GoToCenter()
    {
        foreach (var page in _pages)
        {
            page.ResetMove();
        }
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnDemandHomeScreenPanel>(OnDemandPageOpen);
        

    }
}

public class OnDemandHomeScreenPanel : ISignal
{
    public int HomePageToOpen = 1;
    public UnityAction OnCompleteAction;
}