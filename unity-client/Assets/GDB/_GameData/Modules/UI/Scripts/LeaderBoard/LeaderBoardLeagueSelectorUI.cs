using System;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardLeagueSelectorUI : MonoBehaviour
{
    [SerializeField] private LeagueCategoryButtonUI[] selectorButtons;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite unselectedSprite;
    private void Awake()
    {
        for (int i = 0; i < selectorButtons.Length; i++)
        {
            int index = i; 
        
            selectorButtons[i].GetComponent<Button>().onClick.AddListener(() => 
            { 
                OnSelected(index); // Use the local copy
            });
        }
    
        OnSelected(0);
    }

    void OnSelected(int id)
    {
        Debug.LogError("ON Selected :: "+id);
        for (int i = 0; i < selectorButtons.Length; i++){
            selectorButtons[i].OnSelected(i == id);
            selectorButtons[i].GetComponent<Image>().sprite = i == id ?  selectedSprite : unselectedSprite;
        }
    }
}
