using Gs2.Sample.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WebViewDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title = null;
    [SerializeField] private TextMeshProUGUI text = null;
    [SerializeField] private Button closeButton;

    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectOffset padding;

    private WebViewObject webViewObject;

    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Initialize()
    {
        if (!webViewObject)
        {
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init(
                enableWKWebView: true
            );
            float coefficient = 0.0f;
            Vector2 reference = canvasScaler.referenceResolution;
            int horizon = (int)((Screen.width - (reference.x * Screen.height / reference.y)) / 2);
            int vertical = (int)((Screen.height - (reference.y * Screen.width / reference.x)) / 2);
            if (horizon < 0)
            {
                horizon = 0;
                coefficient = (float)Screen.width / reference.x;
            }
            if (vertical < 0)
            {
                vertical = 0;
                coefficient = (float)Screen.height / reference.y;
            }
            if (coefficient == 0)
            {
                coefficient = (float)Screen.width / reference.x;
            }
            webViewObject.SetMargins(
                left: horizon + (int)(padding.left * coefficient),
                top: vertical + (int)(padding.top * coefficient),
                right: horizon + (int)(padding.right * coefficient),
                bottom: vertical + (int)(padding.bottom * coefficient)
            );
        }
    }
    
    public void Show(string _title, string url)
    {
        Initialize();
        webViewObject.gameObject.SetActive(true);
        webViewObject.LoadURL(url);
        webViewObject.SetVisibility(true);
        
        title.SetText(_title);
        
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
        text.SetText("Open : " + url);
#endif
        
        this.gameObject.SetActive(true);
    }
    
    public void Init(string _title)
    {
        Initialize();
        
        title.SetText(_title);
    }

    public void SetCookie(string key, string value)
    {
        if (webViewObject != null)
        {
            webViewObject.EvaluateJS("document.cookie = '" + key + "=" + value + "';");
        }
        webViewObject.SaveCookies();
    }
    
    public bool isWebViewActiveAndEnabled()
    {
        return webViewObject.isActiveAndEnabled;
    }
    
    public void LoadURL(string url)
    {
        Initialize();
        webViewObject.gameObject.SetActive(true);
        webViewObject.LoadURL(url);
        webViewObject.SetVisibility(true);
        
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
        text.SetText("Open : " + url);
#endif
        
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        webViewObject.ClearCookies();
        
        webViewObject.gameObject.SetActive(false);
        webViewObject.SetVisibility(false);
        this.gameObject.SetActive(false);
    }
}