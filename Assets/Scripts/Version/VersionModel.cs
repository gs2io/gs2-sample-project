using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Version.Model;
using Gs2.Unity.Gs2Version.Result;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Version
{
    [Serializable]
    public class VersionModel: MonoBehaviour
    {
        public EzVersionModel Model;
        
        public IEnumerator CheckVersion(
            Client client,
            GameSession session,
            string versionNamespaceName,
            string versionName,
            int major,
            int minor,
            int micro,
            CheckVersionEvent onCheckVersion,
            ErrorEvent onError
        )
        {
            AsyncResult<EzCheckVersionResult> result = null;
            
            List<EzTargetVersion> targetVersions = new List<EzTargetVersion>();
            EzTargetVersion targetVersion = new EzTargetVersion();
            targetVersion.VersionName = versionName;
            
            EzVersion version = new EzVersion();
            version.Major = major;
            version.Minor = minor;
            version.Micro = micro;
            targetVersion.Version = version;
            targetVersions.Add(targetVersion);
            
            yield return client.Version.CheckVersion(
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
                versionNamespaceName,   //  ネームスペース名
                targetVersions   //  比較する現在のアプリのバージョン値
            );
            
            if (result != null &&result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                onCheckVersion.Invoke(targetVersions, null);
                yield break;
            }
            
            var version_result = result.Result;
            
            onCheckVersion.Invoke(targetVersions, version_result);
        }
    }
}