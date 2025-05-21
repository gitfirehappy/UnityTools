using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _objectToSpawn;
    [SerializeField] private Transform _parent;
    [SerializeField] public float gravity = 1f;

    private void Update()
    {
        if (Input.GetMouseButton(0)) // 0 = 左键按下
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Rigidbody2D rb = ObjectPoolManager.SpawnObject(_objectToSpawn, _parent, Quaternion.identity,ObjectPoolManager.PoolType.ParticleSystems);
            //rb.gravityScale = 1f;
            //
            //ObjectPoolManager.SpawnObject(_objectToSpawn, worldPos,Quaternion.identity);

            Rigidbody2D rb = ObjectPoolManager.SpawnObject(_objectToSpawn, worldPos, Quaternion.identity, ObjectPoolManager.PoolType.ParticleSystems);
            rb.gravityScale = gravity;
        }
    }

}
