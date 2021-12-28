using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Version.Model;
using Gs2.Unity.Gs2Version.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Version
{
    [Serializable]
    public class TermModel: MonoBehaviour
    {
        public EzVersionModel Model;

        /// <summary>
        /// サーバ側で規約のバージョンマスターと承認済みのバージョンを比較し
        /// 未承認のバージョンをErrorsとWarningsに返す
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="versionNamespaceName"></param>
        /// <param name="versionName"></param>
        /// <param name="onCheckVersion"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator CheckTerm(
            Client client,
            GameSession session,
            string versionNamespaceName,
            string versionName,
            CheckVersionEvent onCheckVersion,
            ErrorEvent onError
        )
        {
            AsyncResult<EzCheckVersionResult> result = null;
            
            List<EzTargetVersion> targetVersions = new List<EzTargetVersion>();
            EzTargetVersion targetVersion = new EzTargetVersion();
            targetVersion.VersionName = versionName;
            
            EzVersion version = new EzVersion();
            version.Major = 0;
            version.Minor = 0;
            version.Micro = 0;
            targetVersion.Version = version;
            targetVersion.VersionName = versionName;
            targetVersions.Add(targetVersion);
            
            yield return client.Version.CheckVersion(
                r =>
                {
                    result = r;
                },
                session,    // GameSession ログイン状態を表すセッションオブジェクト
                versionNamespaceName,   //  ネームスペース名
                targetVersions
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

        public IEnumerator AcceptTerm(
            Client client,
            GameSession session,
            string versionNamespaceName,
            string versionName,
            UnityEvent onAcceptTerm,
            ErrorEvent onError
        )
        {
            // バージョンを承認
            AsyncResult<EzAcceptResult> result = null;
            var current = client.Version.Accept(
                r =>
                {
                    result = r;
                },
                session,
                namespaceName: versionNamespaceName,
                versionName: versionName
            );

            yield return current;
            if (result != null &&result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }
            
            onAcceptTerm.Invoke();
        }
    }
}