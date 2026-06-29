using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;

public class ObjectPoolService
{
  private readonly Dictionary<Component, object> _prefabToPoolMap = new();
  private readonly Dictionary<Component, Action<Component>> _instanceToReleaseMap = new();
  private readonly Transform _poolRoot;

  public ObjectPoolService(Transform poolRoot = null)
  {
    if (poolRoot == null)
    {
      GameObject rootGo = new GameObject("[ObjectPoolService_Root]");
      _poolRoot = rootGo.transform;
      UnityEngine.Object.DontDestroyOnLoad(rootGo);
    }
    else
    {
      _poolRoot = poolRoot;
    }
  }

  /// <summary>
  /// Spawns or reuses a component instance from the pool.
  /// </summary>
  public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
  {
    ObjectPool<T> pool = GetOrCreatePool(prefab);
    T instance = pool.Get();
    Transform targetParent = parent != null ? parent : _poolRoot;
    instance.transform.SetParent(targetParent);
    instance.transform.SetPositionAndRotation(position, rotation);

    return instance;
  }

  /// <summary>
  /// Spawns or reuses a component instance using the prefab's default position and rotation.
  /// </summary>
  public T Get<T>(T prefab, Transform parent = null) where T : Component
  {
    return Get(prefab, prefab.transform.position, prefab.transform.rotation, parent);
  }

  /// <summary>
  /// Returns an instance safely back to its respective object pool.
  /// </summary>
  public void Release<T>(T instance) where T : Component
  {
    if (instance == null) return;

    if (_instanceToReleaseMap.TryGetValue(instance, out Action<Component> releaseAction))
    {
      releaseAction(instance);
    }
    else
    {
      // Fallback safety layer: if it wasn't tracked, just destroy it
      UnityEngine.Object.Destroy(instance.gameObject);
    }
  }

  private ObjectPool<T> GetOrCreatePool<T>(T prefab) where T : Component
  {
    if (_prefabToPoolMap.TryGetValue(prefab, out object existingPool))
    {
      return (ObjectPool<T>)existingPool;
    }

    ObjectPool<T> pool = new ObjectPool<T>(
        createFunc: () =>
        {
          T instance = UnityEngine.Object.Instantiate(prefab, _poolRoot);
          // Map this exact instance back to its release routine
          _instanceToReleaseMap[instance] = (obj) => ReleaseInternal(prefab, (T)obj);
          return instance;
        },
        actionOnGet: (obj) => obj.gameObject.SetActive(true),
        actionOnRelease: (obj) =>
        {
          obj.gameObject.SetActive(false);
          obj.transform.SetParent(_poolRoot);
        },
        actionOnDestroy: (obj) =>
        {
          _instanceToReleaseMap.Remove(obj);
          if (obj != null)
          {
            UnityEngine.Object.Destroy(obj.gameObject);
          }
        },
        collectionCheck: true,
        defaultCapacity: 20,
        maxSize: 500
    );

    _prefabToPoolMap.Add(prefab, pool);
    return pool;
  }

  private void ReleaseInternal<T>(T prefab, T instance) where T : Component
  {
    if (_prefabToPoolMap.TryGetValue(prefab, out object poolObj) && poolObj is ObjectPool<T> pool)
    {
      pool.Release(instance);
    }
  }

  public void PlayParticleAndForget(ParticleSystem prefab, Vector3 position, Quaternion rotation, Transform parent = null)
  {
    if (prefab == null) return;

    PlayParticleAsyncInternal(prefab, position, rotation, parent).Forget();
  }

  private async UniTaskVoid PlayParticleAsyncInternal(ParticleSystem prefab, Vector3 position, Quaternion rotation, Transform parent)
  {
    // Fetch from pool and set placement
    ParticleSystem instance = Get(prefab, position, rotation, parent);

    // Play the particle system
    instance.Play(withChildren: true);

    // Keep tracking in the background until it completely runs out of active particles
    // 'true' ensures it checks all nested child particle paths as well
    await UniTask.WaitWhile(() => instance != null && instance.IsAlive(withChildren: true));

    // 4. Return safely back to the pool matrix
    if (instance != null)
    {
      Release(instance);
    }
  }
}