using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeithCodes.Pooling
{
    public class ObjectPooler : MonoBehaviour
    {
        /// <summary>
        /// If preloadedObjects is greater than 0, on Awake, Object Pools
        /// for each prefab in the prefab pool will be initialized
        /// and create that many corresponding (inactive) objects
        /// </summary>
        [SerializeField, Range(0, 100)]
        private int preloadedObjects = 0;

        [SerializeField]
        private PrefabPoolContainer prefabPoolContainer;

        private static Dictionary<string, GameObject> prefabPool;
        private static Dictionary<string, ObjectPool> objectPools = new Dictionary<string, ObjectPool>();

        private static Transform objectPoolContainer;

        private void Awake()
        {
            objectPoolContainer = transform;
            bool provided = providePrefabPool(prefabPoolContainer.GetDictionary());
            Debug.Log("Prefab pool provided: " + provided);
            if (preloadedObjects == 0)
                return;
            foreach(KeyValuePair<string, GameObject> pair in prefabPool)
                objectPools.Add(pair.Key, createObjectPool(pair.Key, preloadedObjects));
        }

        private static bool providePrefabPool(Dictionary<string, GameObject> providedPool)
        {
            if(prefabPool != null)
            {
                objectPools = new Dictionary<string, ObjectPool>();
                Debug.Log("Resetting Object Pools");
            }
            prefabPool = providedPool;
            return true;
        }

        public static GameObject GetObjectFromPool(string poolKey)
        {
            if(!objectPools.ContainsKey(poolKey))
            {
                objectPools.Add(poolKey, createObjectPool(poolKey));
            }
            Debug.Log("Returning object from pool: " + poolKey);
            return objectPools[poolKey].GetObjectFromPool();
        }

        public static bool ReturnObjectToPool(GameObject objectToReturn)
        {
            string poolKey = objectToReturn.name.Remove(objectToReturn.name.Length - "(Clone)".Length);
            return ReturnObjectToPool(objectToReturn, poolKey);
        }

        public static bool ReturnObjectToPool(GameObject objectToReturn, string poolKey)
        {
            if (!objectPools.ContainsKey(poolKey))
            {
                objectPools.Add(poolKey, createObjectPool(poolKey));
            }
            Debug.Log("Returning object to pool: " + poolKey);
            objectPools[poolKey].DeactivateObject(objectToReturn);
            return true;
        }

        private static ObjectPool createObjectPool(string poolKey, int objectsToPreload = 0)
        {
            GameObject newPoolObject = new GameObject(poolKey + " Pool", typeof(ObjectPool));
            newPoolObject.transform.SetParent(objectPoolContainer);
            ObjectPool newPool = newPoolObject.GetComponent<ObjectPool>();
            newPool.Initialize(prefabPool[poolKey], objectsToPreload);
            return newPool;
        }
    }
}
