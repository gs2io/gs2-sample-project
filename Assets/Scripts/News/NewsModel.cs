using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2News.Model;
using Gs2.Unity.Gs2News.Result;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

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
            Gs2Domain gs2,
            GameSession gameSession,
            string newsNamespaceName,
            GetContentsUrlEvent onGetContentsUrl,
            ErrorEvent onError
        )
        {
            var domain = gs2.News.Namespace(
                namespaceName: newsNamespaceName
            ).Me(
                gameSession: gameSession
            ).News(
            );
            var future = domain.GetContentsUrl();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error,
                    null
                );
                yield break;
            }
            
            var items = future.Result.ToList();
            foreach (var item in items)
            {
                var future2 = item.Model();
                yield return future2;
                var entry = future2.Result;
                cookies.Add(entry);
            }
            browserUrl = domain.BrowserUrl;
            zipUrl = domain.ZipUrl;
            
            onGetContentsUrl.Invoke(cookies, browserUrl, zipUrl);
        }
        
#if GS2_ENABLE_UNITASK
        public async UniTask GetContentsUrlAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string newsNamespaceName,
            GetContentsUrlEvent onGetContentsUrl,
            ErrorEvent onError
        )
        {
            var domain = gs2.News.Namespace(
                namespaceName: newsNamespaceName
            ).Me(
                gameSession: gameSession
            ).News();
            var result = await domain.GetContentsUrlAsync();

            var items = result.ToList();
            foreach (var item in items)
            {
                var entry = await item.ModelAsync();
                cookies.Add(entry);
            }
            browserUrl = domain.BrowserUrl;
            zipUrl = domain.ZipUrl;
            
            onGetContentsUrl.Invoke(cookies, browserUrl, zipUrl);
        }
#endif
        
        public IEnumerator ListNewses(
            Gs2Domain gs2,
            GameSession gameSession,
            string newsNamespaceName,
            GetListNewsesEvent onListNewses,
            ErrorEvent onError
        )
        {
            var domain = gs2.News.Namespace(
                namespaceName: newsNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var it = domain.Newses();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    break;
                }

                if (it.Current != null)
                {
                    news.Add(it.Current);
                }
            }
            
            contentHash = domain.ContentHash;
            templateHash = domain.TemplateHash;
            
            onListNewses.Invoke(news, contentHash, templateHash);
        }
        
#if GS2_ENABLE_UNITASK
        public async UniTask ListNewsesAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string newsNamespaceName,
            GetListNewsesEvent onListNewses,
            ErrorEvent onError
        )
        {
            var domain = gs2.News.Namespace(
                namespaceName: newsNamespaceName
            ).Me(
                gameSession: gameSession
            );
            news = await domain.NewsesAsync().ToListAsync();
            
            contentHash = domain.ContentHash;
            templateHash = domain.TemplateHash;
            
            onListNewses.Invoke(news, contentHash, templateHash);
        }
#endif
    }
}