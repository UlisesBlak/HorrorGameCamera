using System.Threading;
using UnityEngine;

namespace Utils
{
    [DefaultExecutionOrder(-50)]
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        private static readonly object _lock = new object();
        public static bool applicationIsQuitting = false;
        private static bool _isCreating = false;
        private static bool _isDuplicate = false;
        
        public static bool HasInstance => applicationIsQuitting == false && _instance != null;
        public static T TryGetInstance() => HasInstance ? Instance : null;

        public static T Instance
        {
            get {
                if (applicationIsQuitting) {
                    EDebug.LogError($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (_lock) {
                    if (_instance) return _instance;
                    
                    if (_isCreating) {
                        while (!_instance) {
                            Monitor.Wait(_lock);
                        }
                        return _instance;
                    }

                    _isCreating = true;
                    _instance = (T)FindFirstObjectByType(typeof(T));

                    if (FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1) {
                        EDebug.LogError($"[Singleton] Something went really wrong {typeof(T)}");
                        _isCreating = false;
                        Monitor.PulseAll(_lock);
                        return _instance;
                    }

                    if (!_instance) {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = $"(singleton) {typeof(T)}";
                        DontDestroyOnLoad(singleton);
                        EDebug.Log("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" 
                            + singleton + "' was created with DontDestroyOnLoad.");
                    }
                    else EDebug.Log($"[Singleton] Using instance already created: {_instance.gameObject.name}");
                        
                    _isCreating = false;
                    Monitor.PulseAll(_lock);

                    return _instance;
                }
            }
        }

        private void Awake()
        {
            if (_instance && _instance != this) {
                EDebug.LogError($"[Singleton] Duplicate instance of {typeof(T)} found. Destroying: {gameObject.name}");
                _isDuplicate = true;
                Destroy(gameObject);
                return;
            }
            if (!_instance) {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            OnAwake();
        }
        
        protected virtual void OnAwake()
        {
            EDebug.LogGood($"{gameObject.name} ► Initialized");
        }

        private void OnDestroy()
        {
            if (_isDuplicate) return;
            if (_instance == this) _instance = null;
            EDebug.LogWarning($"[Singleton] Main instance of {typeof(T)} destroyed. Instance cleared.");
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
            _instance = null;
            EDebug.LogWarning($"The Singleton: {typeof(T)} is quitting. Be careful");
        }
        
    }
}
