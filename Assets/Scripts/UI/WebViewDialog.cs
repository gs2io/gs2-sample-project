//#define USE_UNIWEBVIEW // Enable this when using UniWebView.

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

#if USE_UNIWEBVIEW
    private UniWebView webViewObject;
#else
    private WebViewObject webViewObject;
#endif

    public UnityEvent OnLoaded = new UnityEvent();

    private bool isLoading;
    
    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(Hide);
    }

    public bool UseUniWebView()
    {
#if USE_UNIWEBVIEW
        return true;
#else
        return false;
#endif
    }
#if USE_UNIWEBVIEW
    private void Initialize()
    {
        if (!webViewObject)
        {
            webViewObject = new GameObject("WebViewObject").AddComponent<UniWebView>();
            
            float coefficient = 0.0f;
            Vector2 reference = canvasScaler.referenceResolution;
            int horizon = (int)((Screen.width - (reference.x * Screen.height / reference.y)) / 2);
            int vertical = (int)((Screen.height - (reference.y * Screen.width / reference.x)) / 2);
            if (horizon < 0)
            {
                horizon = 0;
                coefficient = Screen.width / reference.x;
            }
            if (vertical < 0)
            {
                vertical = 0;
                coefficient = Screen.height / reference.y;
            }
            if (coefficient == 0)
            {
                coefficient = Screen.width / reference.x;
            }
            webViewObject.Frame = new Rect(
                horizon + (int)(padding.left * coefficient),
                vertical + (int)(padding.top * coefficient),
                Screen.width - (horizon + (int)(padding.left * coefficient)) - (horizon + (int)(padding.right * coefficient)) ,
                Screen.height - (vertical + (int)(padding.top * coefficient)) - (vertical + (int)(padding.bottom * coefficient))
            );
            
		    webViewObject.Hide();
		    webViewObject.gameObject.SetActive(true);
        }
	    else
	    {
		    webViewObject.Hide();
		    webViewObject.gameObject.SetActive(true);
	    }
    }
#else
    private void Initialize()
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
				},
				err: (msg) => {
				    Debug.Log("Error : " + msg);
				},
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
            
			webViewObject.SetVisibility(false);
			webViewObject.gameObject.SetActive(true);
        }
		else
		{
			webViewObject.SetVisibility(false);
			webViewObject.gameObject.SetActive(true);
		}
    }
#endif

    public void Init(string _title)
    {
        Initialize();
        
        title.SetText(_title);
    }
    
    public void SetCookie(string url, string key, string value)
    {
#if !USE_UNIWEBVIEW
        ;
#else
        UniWebView.SetCookie(url, key + "=" + value + "; path=/;");
#endif
    }
    
    public void ClearCookie()
    {
#if !USE_UNIWEBVIEW
        webViewObject.ClearCookies();
#else
        UniWebView.ClearCookies();
#endif
    }
	
    public bool IsActiveAndEnabled()
    {
        return webViewObject.isActiveAndEnabled;
    }
    
    public void LoadURL(string url)
    {
#if !USE_UNIWEBVIEW
        webViewObject.LoadURL(url);
#else
        webViewObject.Load(url);
#endif
		
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
        text.SetText("Open : " + url);
#endif
#if !USE_UNIWEBVIEW
	    isLoading = true;
#endif
    }
    
    public void LoadHTML(string html, string baseUrl)
    {
#if !USE_UNIWEBVIEW
        webViewObject.LoadHTML(html, baseUrl);
#else
        webViewObject.LoadHTMLString(html, baseUrl);
#endif
		
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
        text.SetText("Open : " + html);
#endif
#if !USE_UNIWEBVIEW
        isLoading = true;
#endif
    }
    
    public bool IsLoading()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
        return false;
#else
        return isLoading;
#endif
    }
	
	public void Reload()
    {
        webViewObject.Reload();
    }
	
    public void SetVisibility(bool visible)
    {
#if !USE_UNIWEBVIEW
        webViewObject.SetVisibility(visible);
#else
        if (visible)
            webViewObject.Show();
        else
            webViewObject.Hide();
#endif
        this.gameObject.SetActive(visible);
    }

    public void Hide()
    {
#if !USE_UNIWEBVIEW
        webViewObject.SetVisibility(false);
#else
        webViewObject.Hide();
#endif
        this.gameObject.SetActive(false);
		
        webViewObject.gameObject.SetActive(false);
	}
}