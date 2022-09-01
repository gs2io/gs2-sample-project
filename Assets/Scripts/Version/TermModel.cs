using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Version.Model;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Version
{
    [Serializable]
    public class TermModel: MonoBehaviour
    {
        public EzVersionModel Model;

        /// <summary>
        /// サーバ側で規約のバージョンマスターと承認済みのバージョンを比較し
        /// 未承認のバージョンをErrorsとWarningsに返す
        /// Server compares the version master of the agreement with the approved version and returns
        /// the unapproved version to Errors and Warnings.
        /// </summary>
        public IEnumerator CheckTerm(
            Gs2Domain gs2,
            GameSession gameSession,
            string versionNamespaceName,
            string versionName,
            CheckVersionEvent onCheckVersion,
            ErrorEvent onError
        )
        {
            List<EzTargetVersion> targetVersions = new List<EzTargetVersion>();
            EzTargetVersion targetVersion = new EzTargetVersion();
            targetVersion.VersionName = versionName;
            
            EzVersion version = new EzVersion();
            version.Major = 0;
            version.Minor = 0;
            version.Micro = 0;
            targetVersion.Version = version;
            targetVersions.Add(targetVersion);
            
            var domain = gs2.Version.Namespace(
                namespaceName: versionNamespaceName
            ).Me(
                gameSession: gameSession
            ).Checker();
            var future = domain.CheckVersion(
                targetVersions: targetVersions.ToArray()
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error,
                    null
                );
                yield break;
            }

            var projectToken = future.Result.ProjectToken;
            var warnings = future.Result.Warnings;
            var errors = future.Result.Errors;
            
            onCheckVersion.Invoke(projectToken, warnings.ToList(), errors.ToList());
        }
#if GS2_ENABLE_UNITASK
        public async UniTask CheckTermAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string versionNamespaceName,
            string versionName,
            CheckVersionEvent onCheckVersion,
            ErrorEvent onError
        )
        {
            List<EzTargetVersion> targetVersions = new List<EzTargetVersion>();
            EzTargetVersion targetVersion = new EzTargetVersion();
            targetVersion.VersionName = versionName;
            
            EzVersion version = new EzVersion();
            version.Major = 0;
            version.Minor = 0;
            version.Micro = 0;
            targetVersion.Version = version;
            targetVersions.Add(targetVersion);
            
            var domain = gs2.Version.Namespace(
                namespaceName: versionNamespaceName
            ).Me(
                gameSession: gameSession
            ).Checker();
            try
            {
                var result = await domain.CheckVersionAsync(
                    targetVersions: targetVersions.ToArray()
                );
                
                var projectToken = result.ProjectToken;
                var warnings = result.Warnings;
                var errors = result.Errors;
            
                onCheckVersion.Invoke(projectToken, warnings.ToList(), errors.ToList());
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif
        
        public IEnumerator AcceptTerm(
            Gs2Domain gs2,
            GameSession gameSession,
            string versionNamespaceName,
            string versionName,
            AcceptTermEvent onAcceptTerm,
            ErrorEvent onError
        )
        {
            var domain = gs2.Version.Namespace(
                namespaceName: versionNamespaceName
            ).Me(
                gameSession: gameSession
            ).AcceptVersion(
                versionName: versionName
            );
            var future = domain.Accept();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error,
                    null
                );
                yield break;
            }

            var future2 = future.Result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(
                    future2.Error,
                    null
                );
                yield break;
            }

            var item = future2.Result;
            
            onAcceptTerm.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask AcceptTermAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string versionNamespaceName,
            string versionName,
            AcceptTermEvent onAcceptTerm,
            ErrorEvent onError
        )
        {
            var domain = gs2.Version.Namespace(
                namespaceName: versionNamespaceName
            ).Me(
                gameSession: gameSession
            ).AcceptVersion(
                versionName: versionName
            );
            try
            {
                var result = await domain.AcceptAsync();
                var item = await result.ModelAsync();
                
                onAcceptTerm.Invoke(item);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif
    }
}