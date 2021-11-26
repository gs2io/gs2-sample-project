using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogWindow : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI logText = null;
    
    [SerializeField]
    private ScrollRect scrollRect = null;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLog(string text)
    {
        logText.SetText(logText.text + text + "\n");

        if (gameObject.activeInHierarchy)
        {
            // スクロールバーを一番下に
            StartCoroutine(ForceScrollDown());
        }
    }
    
    IEnumerator ForceScrollDown()

    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0.0f;
    }
}
