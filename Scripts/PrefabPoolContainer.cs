using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeithComet.Pooling
{
    public class PrefabPoolContainer : ScriptableObject
    {

        [SerializeField]
        private GameObject[] prefabs;

        [SerializeField]
        private string[] prefabKeys;

        public void Initialize(Dictionary<string, GameObject> spriteDict)
        {
            Debug.Log("Initializing prefab container");
            prefabKeys = new string[spriteDict.Count];
            prefabs = new GameObject[spriteDict.Count];
            int i = 0;
            foreach (KeyValuePair<string, GameObject> pair in spriteDict)
            {
                prefabKeys[i] = pair.Key;
                prefabs[i] = pair.Value;
                i++;
                Debug.Log("Initializing " + pair.Key + " :" + pair.Value);
            }
        }

        public Dictionary<string, GameObject> GetDictionary()
        {
            Dictionary<string, GameObject> dict = new Dictionary<string, GameObject>();
            for (int i = 0; i < prefabKeys.Length; i++)
            {
                dict.Add(prefabKeys[i], prefabs[i]);
            }
            return dict;
        }
    }
}