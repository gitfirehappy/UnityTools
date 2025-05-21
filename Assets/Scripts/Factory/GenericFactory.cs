using System.Collections.Generic;
using UnityEngine;

public static class GenericFactory<T> where T : UnityEngine.Object
{
    private static readonly Dictionary<string, T> _cache = new();

    /// <summary>
    /// 从 Resources 路径加载并实例化一个对象
    /// </summary>
    public static T Create(string resourcePath, Vector3? position = null, Quaternion? rotation = null)
    {
        if (!_cache.TryGetValue(resourcePath, out var prefab))
        {
            prefab = Resources.Load<T>(resourcePath);
            if (prefab == null)
            {
                Debug.LogError($"[GenericFactory] 无法从路径 '{resourcePath}' 加载类型 {typeof(T)}");
                return null;
            }
            _cache[resourcePath] = prefab;
        }

        if (typeof(T) == typeof(GameObject))
        {
            return Object.Instantiate(prefab, position ?? Vector3.zero, rotation ?? Quaternion.identity) as T;
        }
        else
        {
            return Object.Instantiate(prefab);
        }
    }

    /// <summary>
    /// 仅加载资源（不实例化），适合 ScriptableObject
    /// </summary>
    public static T Load(string resourcePath)
    {
        if (!_cache.TryGetValue(resourcePath, out var asset))
        {
            asset = Resources.Load<T>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[GenericFactory] 无法加载资源路径 '{resourcePath}'");
                return null;
            }
            _cache[resourcePath] = asset;
        }

        return asset;
    }
}
