using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectPoolManager : SingletonMono<ObjectPoolManager>
{
    #region 🛠️ 公共配置参数

    [Header("Pool Config")]
    [SerializeField] private bool _addToDontDestroyOnLoad = false;
    [SerializeField] private int _defaultPoolCapacity = 10;
    [SerializeField] private int _maxPoolSize = 100;
    [SerializeField] private bool _showDebugInfo = true; // ✅ 控制是否显示调试信息

    #endregion

    #region 🧱 池对象管理

    private GameObject _emptyHolder;

    private static GameObject _particleSystemsEmpty;
    private static GameObject _gameObjectsEmpty;
    private static GameObject _soundFXEmpty;

    private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;
    private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

    public enum PoolType { ParticleSystems, GameObjects, SoundFX }
    public static PoolType PoolingType;

    #endregion

    #region 🔁 生命周期：初始化
    protected override void Init()
    {
        _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();
        SetupEmpties();
    }
    #endregion

    #region 📦 初始化空节点结构
    private void SetupEmpties()
    {
        _emptyHolder = new GameObject("Object Pools");

        _particleSystemsEmpty = new GameObject("Particle Effects");
        _particleSystemsEmpty.transform.SetParent(_emptyHolder.transform);

        _gameObjectsEmpty = new GameObject("GameObjects");
        _gameObjectsEmpty.transform.SetParent(_emptyHolder.transform);

        _soundFXEmpty = new GameObject("Sound FX");
        _soundFXEmpty.transform.SetParent(_emptyHolder.transform);

        if (_addToDontDestroyOnLoad)
            DontDestroyOnLoad(_emptyHolder);
    }
    #endregion

    #region 🧰 创建对象池

    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType)
    {
        var pool = new ObjectPool<GameObject>(
            () => CreateObject(prefab, pos, rot, poolType),
            OnGetObject,
            OnReleaseObject,
            OnDestroyObject,
            false,
            Instance._defaultPoolCapacity,
            Instance._maxPoolSize
        );
        _objectPools.Add(prefab, pool);
    }

    private static void CreatePool(GameObject prefab, Transform parent, Quaternion rot, PoolType poolType)
    {
        var pool = new ObjectPool<GameObject>(
            () => CreateObject(prefab, parent, rot, poolType),
            OnGetObject,
            OnReleaseObject,
            OnDestroyObject,
            false,
            Instance._defaultPoolCapacity,
            Instance._maxPoolSize
        );
        _objectPools.Add(prefab, pool);
    }
    #endregion

    #region 🧱 创建对象本体

    private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType)
    {
        prefab.SetActive(false);
        GameObject obj = Instantiate(prefab, pos, rot);
        prefab.SetActive(true);
        obj.transform.SetParent(SetParentObject(poolType).transform);
        return obj;
    }

    private static GameObject CreateObject(GameObject prefab, Transform parent, Quaternion rot, PoolType poolType)
    {
        prefab.SetActive(false);
        GameObject obj = Instantiate(prefab, parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = rot;
        obj.transform.localScale = Vector3.one;
        prefab.SetActive(true);
        return obj;
    }
    #endregion

    #region 🔄 对象生命周期钩子

    private static void OnGetObject(GameObject obj) { }

    protected static void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    private static void OnDestroyObject(GameObject obj)
    {
        if (_cloneToPrefabMap.ContainsKey(obj))
            _cloneToPrefabMap.Remove(obj);
    }
    #endregion

    #region 🧭 池对象父节点绑定
    private static GameObject SetParentObject(PoolType poolType)
    {
        return poolType switch
        {
            PoolType.ParticleSystems => _particleSystemsEmpty,
            PoolType.GameObjects => _gameObjectsEmpty,
            PoolType.SoundFX => _soundFXEmpty,
            _ => null,
        };
    }
    #endregion

    #region 🚀 生成对象（重载）

    private static T SpawnObject<T>(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType) where T : UnityEngine.Object
    {
        if (!_objectPools.ContainsKey(prefab))
            CreatePool(prefab, pos, rot, poolType);

        GameObject obj = _objectPools[prefab].Get();
        if (!_cloneToPrefabMap.ContainsKey(obj))
            _cloneToPrefabMap.Add(obj, prefab);

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);

        return ResolveObject<T>(prefab, obj);
    }

    private static T SpawnObject<T>(GameObject prefab, Transform parent, Quaternion rot, PoolType poolType) where T : UnityEngine.Object
    {
        if (!_objectPools.ContainsKey(prefab))
            CreatePool(prefab, parent, rot, poolType);

        GameObject obj = _objectPools[prefab].Get();
        if (!_cloneToPrefabMap.ContainsKey(obj))
            _cloneToPrefabMap.Add(obj, prefab);

        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = rot;
        obj.SetActive(true);

        return ResolveObject<T>(prefab, obj);
    }

    private static T ResolveObject<T>(GameObject prefab, GameObject obj) where T : UnityEngine.Object
    {
        if (typeof(T) == typeof(GameObject)) return obj as T;

        T component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"Object {prefab.name} doesn't have component of type {typeof(T)}");
            return null;
        }
        return component;
    }
    #endregion

    #region 🔁 外部接口重载

    public static T SpawnObject<T>(T prefab, Vector3 pos, Quaternion rot, PoolType type = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(prefab.gameObject, pos, rot, type);
    }

    public static GameObject SpawnObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType type = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(prefab, pos, rot, type);
    }

    public static T SpawnObject<T>(T prefab, Transform parent, Quaternion rot, PoolType type = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(prefab.gameObject, parent, rot, type);
    }

    public static GameObject SpawnObject(GameObject prefab, Transform parent, Quaternion rot, PoolType type = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(prefab, parent, rot, type);
    }
    #endregion

    #region 🧹 回收对象
    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if (_cloneToPrefabMap.TryGetValue(obj, out var prefab))
        {
            GameObject parentObject = SetParentObject(poolType);
            if (obj.transform.parent != parentObject.transform)
                obj.transform.SetParent(parentObject.transform);

            if (_objectPools.TryGetValue(prefab, out var pool))
                pool.Release(obj);
        }
        else
        {
            Debug.LogError("Trying to return an object that is not pooled: " + obj.name);
        }
    }
    #endregion

    #region 🧪 实时调试信息
#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!_showDebugInfo) return;

        GUIStyle style = new GUIStyle(GUI.skin.box) { fontSize = 12, alignment = TextAnchor.UpperLeft };

        GUILayout.BeginArea(new Rect(10, 10, 400, 400), style);
        GUILayout.Label("[Object Pool Monitor]", EditorStyles.boldLabel);

        foreach (var kvp in _objectPools)
        {
            var prefab = kvp.Key;
            var pool = kvp.Value;
            GUILayout.Label($"Prefab: {prefab.name} | Total: {pool.CountAll} | Inactive: {pool.CountInactive}");
        }

        GUILayout.EndArea();
    }
#endif
    #endregion
}