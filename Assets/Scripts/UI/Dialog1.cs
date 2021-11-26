using System.Collections;
using System.Collections.Generic;
using Gs2.Unity.Gs2Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dialog1 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Title = null;
    [SerializeField] private TextMeshProUGUI Text = null;
    [SerializeField] private Button OKButton = null;
    [SerializeField] private TextMeshProUGUI OKText = null;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(string title, string text, string oktext = "OK")
    {
        Title.SetText(title);
        Text.SetText(text);
        OKText.SetText(oktext);
        
        OKButton.onClick.RemoveAllListeners();
        OKButton.onClick.AddListener(OnCloseEvent);
        
        gameObject.SetActive(true);
    }
    
    public void AddListner(UnityAction callback)
    {
        OKButton.onClick.RemoveAllListeners();
        OKButton.onClick.AddListener(callback);
        OKButton.onClick.AddListener(OnCloseEvent);
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
