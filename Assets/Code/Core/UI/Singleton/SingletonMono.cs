using UnityEngine;
using UnityEngine.UI;

namespace EasyEngine.Base
{
    /// <summary>
    /// Singleton base class.
    /// Derive this class to make it Singleton.
    /// </summary>
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T m_Instance;

        private bool initialized = false;
        /// <summary>
        /// Returns the instance of this singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = (T)FindObjectOfType(typeof(T));

                    if (m_Instance == null)
                    {
                      
                        {
                            GameObject obj = new GameObject(typeof(T).ToString());
                            m_Instance = obj.AddComponent<T>();
                        }
                    }
                }
                return m_Instance;
            }
        }
        
        protected void Awake()
        {
           
            if (m_Instance == null)
            {
                m_Instance = this as T;
            }
            DontDestroyOnLoad(this);
        }
  
        public virtual void Init()
        {
            if(initialized) return;
            initialized = true;
        }

    }
}