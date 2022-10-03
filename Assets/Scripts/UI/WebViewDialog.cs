using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WebViewDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title = null;
    [SerializeField] private TextMeshProUGUI text = null;
    [SerializeField] private Button closeButton;

    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectOffset padding;

    private WebViewObject webViewObject;

    public UnityEvent OnLoaded = new UnityEvent();

    private bool isLoading;
    
    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(Hide);
    }
	
    private void Initialize(UnityAction callback = null)
    {
        if (!webViewObject)
        {
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init(
			    started: (msg) => {
				    Debug.Log("Started : " + msg);
					if (webViewObject.Progress() != 100) {
					    isLoading = true;
					}
				},
				ld: (msg) => {
				    isLoading = false;
				    Debug.Log("Loaded : " + msg);

					OnLoaded.Invoke();
				},
				err: (msg) => {
				    Debug.Log("Error : " + msg);
				},
                enableWKWebView: true
            );
            
            if (callback != null)
	            OnLoaded.AddListener(callback);
            
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
            
			webViewObject.SetVisibility(false);
			webViewObject.gameObject.SetActive(true);
        }
		else
		{
			webViewObject.SetVisibility(false);
			webViewObject.gameObject.SetActive(true);
		}
    }
    
    public void Init(string _title, UnityAction callback = null)
    {
        Initialize(callback);
        
        title.SetText(_title);
    }
    
    public void SetCustomHeader(string headerKey, string headerValue)
    {
	    webViewObject.RemoveCustomHeader(headerKey);
	    webViewObject.AddCustomHeader(headerKey, headerValue);
	    Debug.Log("headerKey : " + headerKey);
	    Debug.Log("headerValue : " + headerValue);
    }

    public void SetCookie(string key, string value)
    {
        webViewObject.EvaluateJS(@"document.cookie = '" + key + "=" + value + "'");
        Debug.Log("key : " + key);
        Debug.Log("value : " + value);
    }
    
    public void SaveCookie()
    {
        webViewObject.SaveCookies();
    }

    public void ClearCookie()
    {
        webViewObject.ClearCookies();
    }
	
    public bool isActiveAndEnabled()
    {
        return webViewObject.isActiveAndEnabled;
    }
    
    public void LoadURL(string url)
    {
        webViewObject.LoadURL(url);
		
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
        text.SetText("Open : " + url);
#endif
	    isLoading = true;
    }
    
    public bool IsLoading()
    {
        return isLoading;
    }
	
	public void Reload()
    {
        webViewObject.Reload();
    }
	
    public void SetVisibility(bool visible)
    {
        webViewObject.SetVisibility(visible);
        this.gameObject.SetActive(visible);
    }

    public void Hide()
    {
        webViewObject.SetVisibility(false);
        this.gameObject.SetActive(false);
		
        webViewObject.gameObject.SetActive(false);
	}
}