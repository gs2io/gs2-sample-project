# News Description

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

````
Folder Composition

gs2-news-sample
 |- archetypes
 |- content
     |- news
     |- events
     |- maintenance
 |- layouts
````

## About WebView

The WebView used in the sample is unity-webview(https://github.com/gree/unity-webview).  
The supported platforms are iOS/Android/mac, and Windows operation is not supported.  
It is installed from the package manager.

## Flow of displaying notifications

### Retrieve URL and cookie value

Obtain from GS2-News the URL to connect to the deployed delivery content and the key and value of the cookie used to verify access privileges.

```c#
AsyncResult<EzGetContentsUrlResult> result = null;
yield return client.News.GetContentsUrl(
    r => {
        if (r.Error ! = null)
        {
            // Reached if an error occurs
            // r.Error contains the exception object that occurred
            Debug.Log(r.Error);
        }
        else
        {
            result = r;
        }
    },
    session, // Session object representing the GameSession login state
    newsNamespaceName // namespace name
);
```

### Open content in WebView

Set each cookie value retrieved to the WebView side, or in the case of unity-webview, pass it from EvaluateJS().

```c#
 webViewObject.EvaluateJS("document.cookie = '" + key + "=" + value + "';");
```

Open the connection URL in WebView.

```c#
webViewObject.LoadURL(url);
```