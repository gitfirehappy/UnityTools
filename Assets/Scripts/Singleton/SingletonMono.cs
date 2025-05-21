using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _quitting = false;
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_quitting)
            {
                Debug.LogWarning($"[SingletonMono] Instance of {typeof(T)} already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject($"{typeof(T)} (Singleton)");
                        _instance = obj.AddComponent<T>();
                        DontDestroyOnLoad(obj);
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            Init(); // 子类可实现 Init()
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _quitting = true;
    }

    /// <summary>
    /// 子类实现此方法代替 override Awake()
    /// </summary>
    protected virtual void Init() { }
}
