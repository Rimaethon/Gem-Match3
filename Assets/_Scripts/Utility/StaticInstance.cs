using System;
using UnityEngine;

namespace Rimaethon.Scripts.Utility
{
    #region Static Instance

    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static bool IsApplicationQuitting { get; set; }

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();


                if (_instance == null && !IsApplicationQuitting)
                    Debug.LogError($"Instance of type {typeof(T)} could not be found.");
                Debug.Log($"Instance of type {typeof(T)} found.");

                return _instance;
            }
            protected set => _instance = value;
        }


        protected virtual void Awake()
        {
            InitializeInstance();
        }

        protected virtual void OnApplicationQuit()
        {
            IsApplicationQuitting = true;
        }

        private void InitializeInstance()
        {
            if (this is T instance)
            {
                Instance = instance;
                Debug.Log($"Instance of type {typeof(T)} created.");
            }
            else
            {
                Debug.LogError($"Instance of type {typeof(T)} could not be created.");
                throw new InvalidOperationException($"Instance of type {typeof(T)} could not be created.");
            }
        }
    }

    #endregion


    #region Singleton

    public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (this is T instance)
            {
                if (Instance != null && Instance != this)
                    Destroy(gameObject);
                else
                    Instance = instance;
            }
            else
            {
                Debug.LogError($"Instance of type {typeof(T)} could not be created.");
                throw new InvalidOperationException($"Instance of type {typeof(T)} could not be created.");
            }
        }
    }

    #endregion


    #region DontDestroyOnLoad

    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }

    #endregion


    #region Private Singleton

    public abstract class PrivateSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        protected virtual void Awake()
        {
            if (this is T instance)
            {
                if (_instance != null && _instance != this)
                    Destroy(gameObject);
                else
                    _instance = instance;
            }
            else
            {
                Debug.LogError($"Instance of type {typeof(T)} could not be created.");
                throw new InvalidOperationException($"Instance of type {typeof(T)} could not be created.");
            }
        }
    }

    #endregion

    public abstract class PrivatePersistentSingleton<T> : PrivateSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}