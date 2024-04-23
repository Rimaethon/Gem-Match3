using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Rimaethon.Scripts.Utility
{
    #region Static Instance

    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;


        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();

                if (_instance != null) return _instance;
                return _instance;
            }
            protected set => _instance = value;
        }


        protected virtual void Awake()
        {
            InitializeInstance();
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
            base.Awake();
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