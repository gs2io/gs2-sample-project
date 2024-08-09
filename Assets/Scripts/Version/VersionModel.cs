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
    public class VersionModel: MonoBehaviour
    {
        public EzVersionModel Model;
        
        public IEnumerator CheckVersion(
            Gs2Domain gs2,
            GameSession gameSession,
            string versionNamespaceName,
            string versionName,
            int major,
            int minor,
            int micro,
            CheckVersionEvent onCheckVersion,
            ErrorEvent onError
        )
        {
            List<EzTargetVersion> targetVersions = new List<EzTargetVersion>();
            EzTargetVersion targetVersion = new EzTargetVersion();
            targetVersion.VersionName = versionName;
            
            EzVersion version = new EzVersion();
            version.Major = major;
            version.Minor = minor;
            version.Micro = micro;
            targetVersion.Version = version;
            targetVersions.Add(targetVersion);
            
            var domain = gs2.Version.Namespace(
                namespaceName: versionNamespaceName
            ).Me(
                gameSession: gameSession
            ).Checker();
            var future = domain.CheckVersionFuture(
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
        public async UniTask CheckVersionAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string versionNamespaceName,
            string versionName,
            int major,
            int minor,
            int micro,
            CheckVersionEvent onCheckVersion,
            ErrorEvent onError
        )
        {
            List<EzTargetVersion> targetVersions = new List<EzTargetVersion>();
            EzTargetVersion targetVersion = new EzTargetVersion();
            targetVersion.VersionName = versionName;
            
            EzVersion version = new EzVersion();
            version.Major = major;
            version.Minor = minor;
            version.Micro = micro;
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
    }
}