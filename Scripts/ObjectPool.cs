using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeithComet.Pooling
{
    public class ObjectPool : MonoBehaviour
    {
        public const int PRELOADED_OBJECTS = 0;

        private GameObject pooledPrefab;
        private Transform activeContainer, inactiveContainer;
        private List<GameObject> activeObjects, availableObjects;

        public void Initialize(GameObject prefab, int objectsToPreload = PRELOADED_OBJECTS)
        {
            pooledPrefab = prefab;
            activeObjects = new List<GameObject>();
            availableObjects = new List<GameObject>();
            activeContainer = new GameObject("Active").transform;
            inactiveContainer = new GameObject("Inactive").transform;
            activeContainer.SetParent(transform);
            inactiveContainer.SetParent(transform);

            if (objectsToPreload == 0)
                return;
            for (int i = 0; i < objectsToPreload; i++)
                GetObjectFromPool();
            DeactivateObjects();
        }

        /// <summary>
        /// Returns an available (inactive) object, or creates one if none available
        /// </summary>
        public GameObject GetObjectFromPool()
        {
            GameObject pooledObject;
            //If there is an available (currently unused) object, grab it
            if (availableObjects.Count > 0)
            {
                pooledObject = availableObjects[0];
                availableObjects.Remove(pooledObject);
                //Debug.Log("grabbed an available object. Remaining: " + availableObjects.Count);
            }
            else
            {
                //Otherwise, generate a new instance of the object
                pooledObject = Instantiate(pooledPrefab);
                //Debug.Log("no available objects. Object created");
            }
            pooledObject.SetActive(true);
            pooledObject.transform.SetParent(activeContainer);
            activeObjects.Add(pooledObject);
            return pooledObject;
        }

        public GameObject GetActiveObject(ObjectPool objectsPool, int index)
        {
            if (activeObjects.Count <= index)
            {
                Debug.Log("No active " + objectsPool.ToString() + " at " + index);
                return null;
            }
            return activeObjects[index];
        }

        public void DeactivateObject(GameObject pooledObject)
        {
            pooledObject.SetActive(false);
            pooledObject.transform.SetParent(inactiveContainer);
            activeObjects.Remove(pooledObject);
            if (!availableObjects.Contains(pooledObject))
                availableObjects.Add(pooledObject);
        }

        public void DeactivateObjects()
        {
            for (int i = 0; i < activeObjects.Count; i++)
            {
                activeObjects[i].SetActive(false);
                activeObjects[i].transform.SetParent(inactiveContainer);
            }
            availableObjects.AddRange(activeObjects);
            activeObjects.Clear();
        }
    }
}