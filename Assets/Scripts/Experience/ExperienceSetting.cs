using System.Collections.Generic;
using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Experience
{
    [System.Serializable]
    public class GetExperienceModelEvent : UnityEvent<string, EzExperienceModel>
    {
    }

    [System.Serializable]
    public class GetStatusesEvent : UnityEvent<EzExperienceModel, List<EzStatus>>
    {
    }

    [System.Serializable]
    public class IncreaseExperienceEvent : UnityEvent<EzExperienceModel, EzStatus, int>
    {
    }
    
    [System.Serializable]
    public class ExperienceSetting : MonoBehaviour
    {
        [SerializeField]
        public string experienceNamespaceName;
        
        [SerializeField]
        public string playerExperienceModelName;

        [SerializeField]
        public string itemExperienceModelName;
        
        [SerializeField]
        public string identifierIncreaseExperienceClientId;

        [SerializeField]
        public string identifierIncreaseExperienceClientSecret;

        [SerializeField]
        public GetExperienceModelEvent onGetExperienceModel = new GetExperienceModelEvent();
        
        [SerializeField]
        public GetStatusesEvent onGetStatuses = new GetStatusesEvent();

        [SerializeField]
        public IncreaseExperienceEvent onIncreaseExperience = new IncreaseExperienceEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}