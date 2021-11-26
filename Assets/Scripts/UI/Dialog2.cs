using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dialog2 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Title = null;
    [SerializeField] private TextMeshProUGUI Text = null;
    [SerializeField] private Button YesButton = null;
    [SerializeField] private TextMeshProUGUI YesText = null;
    [SerializeField] private Button NoButton = null;
    [SerializeField] private TextMeshProUGUI NoText = null;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(string title, string text, string yestext = "Yes", string notext = "No")
    {
        Title.SetText(title);
        Text.SetText(text);
        YesText.SetText(yestext);
        NoText.SetText(notext);
        
        YesButton.onClick.RemoveAllListeners();
        YesButton.onClick.AddListener(OnCloseEvent);
        NoButton.onClick.RemoveAllListeners();
        NoButton.onClick.AddListener(OnCloseEvent);
        
        gameObject.SetActive(true);
    }
    
    public void AddPositiveListener(UnityAction callback)
    {
        YesButton.onClick.RemoveAllListeners();
        YesButton.onClick.AddListener(callback);
        YesButton.onClick.AddListener(OnCloseEvent);
    }
     
    public void AddNegativeListener(UnityAction callback)
    {
        NoButton.onClick.RemoveAllListeners();
        NoButton.onClick.AddListener(callback);
        NoButton.onClick.AddListener(OnCloseEvent);
    }
    
    public void OnCloseEvent()
    {
        gameObject.SetActive(false);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
