using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2News.Model;
using Gs2.Unity.Gs2News.Result;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.News
{
    public class NewsModel : MonoBehaviour
    {
        public List<EzSetCookieRequestEntry> cookies;
        public string browserUrl;
        public string zipUrl;

        public List<EzNews> news;
        public string contentHash;
        public string templateHash;
        
        public IEnumerator GetContentsUrl(
            Client client,
            GameSession session,
            string newsNamespaceName,
            GetContentsUrlEvent onGetContentsUrl,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetContentsUrlResult> result = null;
            yield return client.News.GetContentsUrl(
                r => {
                    if (r.Error != null)
                    {
                        // エラーが発生した場合に到達
                        // r.Error は発生した例外オブジェクトが格納されている
                        Debug.Log(r.Error);
                    }
                    else
                    {
                        result = r;
                    }
                },
                session,    // GameSession ログイン状態を表すセッションオブジェクト
                newsNamespaceName   //  ネームスペース名
            );
            
            if (result != null &&result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }
            
            cookies = result.Result.Items;
            browserUrl = result.Result.BrowserUrl;
            zipUrl = result.Result.ZipUrl;
            
            onGetContentsUrl.Invoke(cookies, browserUrl, zipUrl);
        }
        
        public IEnumerator ListNewses(
            Client client,
            GameSession session,
            string newsNamespaceName,
            GetListNewsesEvent onListNewses,
            ErrorEvent onError
        )
        {
            AsyncResult<EzListNewsesResult> result = null;
            yield return client.News.ListNewses(
                r => {
                    if (r.Error != null)
                    {
                        // エラーが発生した場合に到達
                        // r.Error は発生した例外オブジェクトが格納されている
                        Debug.Log(r.Error);
                    }
                    else
                    {
                        result = r;
                    }
                },
                session,    // GameSession ログイン状態を表すセッションオブジェクト
                newsNamespaceName   //  ネームスペース名
            );
            
            if (result != null &&result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }
            
            news = result.Result.Items;
            contentHash = result.Result.ContentHash;
            templateHash = result.Result.TemplateHash;
            
            onListNewses.Invoke(news, contentHash, templateHash);
        }
    }
}