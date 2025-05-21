using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : new()
{
    private static readonly T _instance = new T();
    public static T Instance => _instance;
}

