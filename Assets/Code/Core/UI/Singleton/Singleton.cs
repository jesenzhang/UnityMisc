using UnityEngine;
using System.Reflection;
using System;

namespace EasyEngine.Base
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        protected static T m_Instance;

        static object m_Lock = new object();
        
        protected Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = typeof(T).InvokeMember(typeof(T).Name, System.Reflection.BindingFlags.CreateInstance | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, null, null) as T;

                        m_Instance.OnSingletonInit();
                    }
                }

                return m_Instance;
            }
        }

        public virtual void Dispose()
        {
            m_Instance = null;
        }

        public virtual void OnSingletonInit()
        {
        }
    }
 
}