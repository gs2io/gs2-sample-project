using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Unit
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] 
        public UnitItem unitItemPrefab;

        [SerializeField] 
        public UnitItemEmpty unitItemEmptyPrefab;

        [SerializeField] 
        public TextMeshProUGUI capacity;

        [SerializeField]
        public Transform contentTransform;
        
        [SerializeField]
        public ScrollRect scrollRect;
        
        public void OnEnable()
        {
            StartCoroutine(ScrollTop());
        }
        private IEnumerator ScrollTop(){
            yield return null;
            scrollRect.verticalNormalizedPosition = 1.0f;
        }
        
        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
    }
}