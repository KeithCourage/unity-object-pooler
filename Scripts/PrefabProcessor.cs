using UnityEngine;
using UnityEditor;
using System;
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
        private static string prefabFileHeader = "namespace KeithComet.Pooling" + Environment.NewLine +
            "{" + Environment.NewLine + "\t" + "public static class PrefabKeys" + System.Environment.NewLine +
            "\t" + "{";
        private static string prefabFileFooter = Environment.NewLine + "\t" + "}" + Environment.NewLine + "}";
        private const string PREFAB_FILE_NAME = "PrefabKeys.cs";

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
            //Check if assets were in/from the object pooler prefab folder
			bool prefabFolderChanged = false;
			foreach (string str in importedAssets)
			{
				if (prefabFolderChanged || isAssetInPrefabFolder(str))
				{
					prefabFolderChanged = true;
					break;
				}
			}
			foreach (string str in deletedAssets)
			{
				if (prefabFolderChanged || isAssetInPrefabFolder(str))
				{
					prefabFolderChanged = true;
					break;
				}
			}
			for (int i = 0; i < movedAssets.Length; i++)
			{
				if (prefabFolderChanged || isAssetInPrefabFolder(movedAssets[i]) || isAssetInPrefabFolder(movedFromAssetPaths[i]))
				{
					prefabFolderChanged = true;
					break;
				}
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
            //locate the PrefabPoolContainer and define needed file paths
            string prefabPoolFilePath = AssetDatabase.FindAssets("PrefabsContainer")[0];
            prefabPoolFilePath = AssetDatabase.GUIDToAssetPath(prefabPoolFilePath);

            PrefabPoolContainer prefabPoolContainer = (PrefabPoolContainer) AssetDatabase.LoadAssetAtPath
                (prefabPoolFilePath, typeof(PrefabPoolContainer));

            prefabPoolFilePath = prefabPoolFilePath.Remove(prefabPoolFilePath.Length - "PrefabsContainer.asset".Length);
            string objectPoolerFolderPath = Application.dataPath;
            objectPoolerFolderPath = objectPoolerFolderPath.Remove(objectPoolerFolderPath.Length - "Assets".Length);
            string prefabFolderPath = objectPoolerFolderPath + prefabPoolFilePath + "Prefabs";

            //create a dictionary of prefabs with keys based on file name
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
            //pass dictionary to PrefabPoolContainer
            prefabPoolContainer.Initialize(prefabs);

            /* write file containing static strings representing prefab pool dictionary keys
             * for use when getting prefabs from ObjectPooler
             */
            string prefabKeyFilePath = prefabPoolFilePath + "Scripts/" + PREFAB_FILE_NAME;
            writePrefabKeysToFile(objectPoolerFolderPath + prefabKeyFilePath, prefabs);
            AssetDatabase.ImportAsset(prefabKeyFilePath);

            Debug.Log("Prefabs Initialized");
        }

        private static void writePrefabKeysToFile(string filePath, Dictionary<string, GameObject> prefabDictionary)
        {
            //create string
            string fileString = prefabFileHeader;
            foreach(KeyValuePair<string, GameObject> pair in prefabDictionary)
            {
                fileString += Environment.NewLine + "\t\t" + "public static string " +
                    pair.Key.Replace(" ", "") + " = " + "\"" + pair.Key + "\";";
            }
            fileString += prefabFileFooter;

            //save file
            File.WriteAllText(filePath, fileString);
        }
	}
}
