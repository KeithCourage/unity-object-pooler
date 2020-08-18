using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace KeithComet.Pooling
{
    /// <summary>
    /// When a file is moved, added, or deleted from the Assets folder, this checks
    /// if the prefabs folder inside the Unity-Object-Pooler folder was involved, and if so,
    /// attempts to add or remove the prefab(s) to the PrefabPoolContainer scriptableObject
    /// </summary>
    public class PrefabProcessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			//Debug.Log("Checking assets");
			bool prefabFolderChanged = false;
			foreach (string str in importedAssets)
			{
				if (prefabFolderChanged || isAssetInPrefabFolder(str))
				{
					prefabFolderChanged = true;
					break;
				}
				//Debug.Log("Reimported Asset: " + str);
			}
			foreach (string str in deletedAssets)
			{
				if (prefabFolderChanged || isAssetInPrefabFolder(str))
				{
					prefabFolderChanged = true;
					break;
				}
				//Debug.Log("Deleted Asset: " + str);
			}
			for (int i = 0; i < movedAssets.Length; i++)
			{
				if (prefabFolderChanged || isAssetInPrefabFolder(movedAssets[i]) || isAssetInPrefabFolder(movedFromAssetPaths[i]))
				{
					prefabFolderChanged = true;
					break;
				}
				//Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
			}

			if (prefabFolderChanged)
				processPrefabs();
		}

		private static bool isAssetInPrefabFolder(string pathOfAsset)
		{
			string lowerCaseAssetPath = pathOfAsset.ToLower();
			if (lowerCaseAssetPath.IndexOf("/unity-object-pooler/prefabs/", System.StringComparison.Ordinal) == -1)
				return false;
			return true;
		}

		private static void processPrefabs()
		{
			Debug.Log("Processing prefabs");
            string prefabPoolFilePath = AssetDatabase.FindAssets("PrefabsContainer")[0];
            prefabPoolFilePath = AssetDatabase.GUIDToAssetPath(prefabPoolFilePath);
            //Debug.Log("path: " + prefabPoolFilePath);

            PrefabPoolContainer prefabPoolContainer = (PrefabPoolContainer) AssetDatabase.LoadAssetAtPath
                (prefabPoolFilePath, typeof(PrefabPoolContainer));

            prefabPoolFilePath = prefabPoolFilePath.Remove(prefabPoolFilePath.Length - "PrefabsContainer.asset".Length);
            string prefabFolderPath = Application.dataPath;
            prefabFolderPath = prefabFolderPath.Remove(prefabFolderPath.Length - "Assets".Length);
            prefabFolderPath += prefabPoolFilePath + "prefabs";

            string[] prefabFiles = Directory.GetFiles(prefabFolderPath, "*.prefab", SearchOption.AllDirectories);
            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
            for (int i = 0; i < prefabFiles.Length; i++)
            {
                string curPath = prefabFiles[i];
                string assetPath = "Assets" + curPath.Replace(Application.dataPath, "").Replace('\\', '/');
                string parentPath = Directory.GetParent(curPath) + "/";
                string prefabName = curPath.Replace(parentPath, "").Replace(".prefab", "");
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                prefabs.Add(prefabName, asset);
            }
            prefabPoolContainer.Initialize(prefabs);
            EditorUtility.SetDirty(prefabPoolContainer);
            Debug.Log("Prefabs Initialized");
        }


	}
}
