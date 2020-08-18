using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeithComet.Pooling
{
    public class ObjectPooler : MonoBehaviour
    {

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

        private static ObjectPool createObjectPool(string poolKey)
        {
            GameObject newPoolObject = new GameObject(poolKey + " Pool", typeof(ObjectPool));
            newPoolObject.transform.SetParent(objectPoolContainer);
            ObjectPool newPool = newPoolObject.GetComponent<ObjectPool>();
            newPool.Initialize(prefabPool[poolKey]);
            return newPool;
        }
    }
}
