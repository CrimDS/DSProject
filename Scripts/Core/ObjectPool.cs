using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int size;
    }

    [Header("Initial Pools")]
    public List<Pool> pools;

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                // --- THE FIX ---
                // Create the object and immediately stamp it with its original prefab.
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(this.transform);
                Projectile p = obj.GetComponent<Projectile>();
                if (p != null) p.SetOriginalPrefab(pool.prefab);

                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.prefab, objectPool);
        }
    }

    public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool with prefab '{prefab.name}' doesn't exist. Creating a new one dynamically.", this);
            poolDictionary.Add(prefab, new Queue<GameObject>());
        }

        GameObject objectToSpawn;

        if (poolDictionary[prefab].Count > 0)
        {
            objectToSpawn = poolDictionary[prefab].Dequeue();
        }
        else
        {
            // Expand the pool and stamp the new object.
            objectToSpawn = Instantiate(prefab);
            objectToSpawn.transform.SetParent(this.transform);
            Projectile p = objectToSpawn.GetComponent<Projectile>();
            if (p != null) p.SetOriginalPrefab(prefab);
        }
        
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject prefab, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"Trying to return object '{objectToReturn.name}' to a pool that doesn't exist for prefab '{prefab.name}'. The object will be destroyed instead.", this);
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[prefab].Enqueue(objectToReturn);
    }
}

