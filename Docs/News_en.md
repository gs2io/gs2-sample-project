# News Explanation

This sample shows how to display notices in WebView (in-app browser) using [GS2-News](https://app.gs2.io/docs/en/index.html#gs2-news).

## GS2-Deploy template

- [initialize_news_template.yaml - mess function](../Templates/initialize_news_template.yaml)

## NewsSetting NewsSetting

![Inspector Window](News.png)

| Setting Name | Description |
|---|---|
| newsNamespaceName | GS2-News namespace name

| Event | Description |
|---|---|
| onGetContentsUrl(List<EzSetCookieRequestEntry>, string, string) | Called when a list of cookies that need to be set to access news articles is obtained. | onGetContentsUrl(List<EzSetCookieRequestEntry>, string, string)
| onGetListNewses(List<EzNews>, string, string>) | Called when a list of news articles is retrieved. | onGetListNewses(List<EzNews>, string, string>)
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

## Prepare content for news distribution

A sample of the content to be displayed in the announcement is available on github at  
[gs2-news-sample](https://github.com/gs2io/gs2-news-sample).  
ZIP the content files downloaded from this page, and then  
Upload the master data from `Import Master Data` in the __GS2-News__ section of the Management Console.

```
Folder Composition

gs2-news-sample
 |- archetypes
 |- content
     |- news
     |- events
     |- maintenance
 |- layouts
 |- config.toml
```

## About WebView

The sample uses the free WebView plug-in [unity-webview](https://github.com/gree/unity-webview) as an environment to try out GS2-News.  
The supported platforms are iOS/Android/mac, and Windows operation is not supported.  
It is installed by the package manager according to the sample manifest.json.  

However, unity-webview lacks some functionality in handling cookies and is not recommended.  
The paid plug-in [UniWebView](https://uniwebview.com/) is the recommended environment (support for external plug-ins is outside the scope of GS2's services).  

If you already have UniWebView, install it in the Assets folder and uncomment the first line of WebViewDialog.cs to work with UniWebView.
```c#
//#define USE_UNIWEBVIEW
```

## Flow of displaying notifications

### Retrieve URL and cookie value

Obtain from GS2-News the URL to connect to the deployed delivery content and the key and value of the cookie used to verify access privileges.

When UniTask is enabled
```c#
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
```
When coroutine is used
```c#
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
        future.Error
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
```

### Open content in WebView

The three retrieved cookies are set on the WebView side.  
In the case of unity-webview, a page for cookie setting in JavaScript is generated, set by passing it to loadHTML, and then the desired page is loaded.

```c#
string html = "<html lang=\"utf-8\"><head><title></title><script>\n";
foreach (var cookie in _newsModel.cookies)
{
    html += String.Format("document.cookie = '{0}={1}; path=/'; \n", cookie.Key, cookie.Value);
}
html += "</script></head><body></body></html>";
UIManager.Instance.LoadHTML(html, _newsModel.browserUrl);
```

Open the destination URL in WebView.。

```c#
webViewObject.LoadURL(url);
```

If it is UniWebView, set the cookie setting on the UniWebView side.

```c#
UniWebView.SetCookie(url, key + "=" + value + "; path=/;");
```

Open the connection URL in WebView.

```c#
webViewObject.Load(url);
```