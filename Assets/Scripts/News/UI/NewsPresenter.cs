using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.News
{
    public class NewsPresenter : MonoBehaviour
    {
        [SerializeField]
        private NewsSetting _newsSetting;
        
        [SerializeField]
        private NewsModel _newsModel;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_newsSetting);
            Assert.IsNotNull(_newsModel);
        }

        /// <summary>
        /// お知らせの初期化
        /// Initialization of Notices
        /// </summary>
        /// <returns></returns>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("NewsPresenter::Initialize");
            
            yield return _newsModel.GetContentsUrl(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _newsSetting.newsNamespaceName,
                _newsSetting.onGetContentsUrl,
                _newsSetting.onError
            );

            yield return _newsModel.ListNewses(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _newsSetting.newsNamespaceName,
                _newsSetting.onGetListNewses,
                _newsSetting.onError
            );
        }
        
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// お知らせの初期化
        /// Initialization of Notices
        /// </summary>
        /// <returns></returns>
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("NewsPresenter::InitializeAync");
            
            await _newsModel.GetContentsUrlAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _newsSetting.newsNamespaceName,
                _newsSetting.onGetContentsUrl,
                _newsSetting.onError
            );

            await _newsModel.ListNewsesAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _newsSetting.newsNamespaceName,
                _newsSetting.onGetListNewses,
                _newsSetting.onError
            );
        }
#endif
        
        /// <summary>
        /// お知らせを開く
        /// Open Notification
        /// </summary>
        /// <returns></returns>
        public IEnumerator OpenWebView()
        {
            UIManager.Instance.AddLog("NewsPresenter::Initialize");
            
            yield return _newsModel.GetContentsUrl(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _newsSetting.newsNamespaceName,
                _newsSetting.onGetContentsUrl,
                _newsSetting.onError
            );

            UIManager.Instance.InitWebViewDialog("Notice");
            while (UIManager.Instance.isWebViewActiveAndEnabled())
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            foreach (var cookie in _newsModel.cookies)
            {
                UIManager.Instance.SetCookie(cookie.Key, cookie.Value);
            }
            UIManager.Instance.LoadURL(_newsModel.browserUrl);
        }
        
        /// <summary>
        /// お知らせを開く
        /// Open Notification
        /// </summary>
        public void OnClickNews()
        {
            StartCoroutine(OpenWebView());
        }

    }
}