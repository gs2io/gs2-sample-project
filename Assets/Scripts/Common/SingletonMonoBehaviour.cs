using System;
using UnityEngine;

namespace Gs2.Sample
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    Type t = typeof(T);
                    instance = (T) FindObjectOfType(t);
                    if (!instance)
                    {
                        Debug.LogError(t + " is nothing.");
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (this != Instance)
            {
                Destroy(this);
                return;
            }
        }
    }
}