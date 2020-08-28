using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace KeithCodes.Pooling
{
    /// <summary>
    /// When a file is moved, added, or deleted from the Assets folder, this checks
    /// if the prefabs folder inside the Unity-Object-Pooler folder was involved, and if so,
    /// attempts to add or remove the prefab(s) to the PrefabPoolContainer scriptableObject
    /// </summary>
    public class PrefabProcessor : AssetPostprocessor
    {
        private const string PREFAB_FOLDER_NAME = "Prefabs/";
        private const string SCRIPTS_FOLDER_NAME = "Scripts/";

        private const string PREFAB_FILE_NAME = "PrefabKeys.cs";
        private static string prefabFileHeader = "namespace KeithComet.Pooling" + Environment.NewLine +
            "{" + Environment.NewLine + "\t" + "public static class PrefabKeys" + Environment.NewLine +
            "\t" + "{";
        private static string prefabFileFooter = Environment.NewLine + "\t" + "}" + Environment.NewLine + "}";

        private static PrefabPoolContainer prefabPoolContainer;
        //location of the folder which contains the PrefabsContainer asset, relative the Assets folder
        private static string objectPoolerFolderPath;
        //location of the object pooler prefabs folder/scripts folder, relative the project folder
        private static string prefabsFolderPath, scriptsFolderPath;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            initialize();

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

        /// <summary>
        /// Locates the PrefabsContainer and defines relevant file paths
        /// </summary>
        private static void initialize()
        {
            objectPoolerFolderPath = AssetDatabase.FindAssets("PrefabsContainer")[0];
            objectPoolerFolderPath = AssetDatabase.GUIDToAssetPath(objectPoolerFolderPath);

            prefabPoolContainer = (PrefabPoolContainer)AssetDatabase.LoadAssetAtPath
                (objectPoolerFolderPath, typeof(PrefabPoolContainer));

            objectPoolerFolderPath = objectPoolerFolderPath.Remove(objectPoolerFolderPath.Length - "PrefabsContainer.asset".Length);
            string projectFolderPath = Application.dataPath.Remove(Application.dataPath.Length - "Assets".Length)
                + objectPoolerFolderPath;
            prefabsFolderPath = projectFolderPath + PREFAB_FOLDER_NAME;
            scriptsFolderPath = projectFolderPath + SCRIPTS_FOLDER_NAME;
        }

        private static bool isAssetInPrefabFolder(string pathOfAsset)
        {
            //Debug.Log("path of asset: " + pathOfAsset);
            if (pathOfAsset.Contains(objectPoolerFolderPath + PREFAB_FOLDER_NAME))
                return true;
            return false;
        }

        private static void processPrefabs()
        {
            //create a dictionary of prefabs with keys based on file name
            string[] prefabFiles = Directory.GetFiles(prefabsFolderPath, "*.prefab", SearchOption.AllDirectories);
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
            writePrefabKeysToFile(scriptsFolderPath + PREFAB_FILE_NAME, prefabs);
            AssetDatabase.ImportAsset(scriptsFolderPath + PREFAB_FILE_NAME);

            Debug.Log("Prefabs Initialized");
        }

        private static void writePrefabKeysToFile(string filePath, Dictionary<string, GameObject> prefabDictionary)
        {
            //create string
            string fileString = prefabFileHeader;
            foreach (KeyValuePair<string, GameObject> pair in prefabDictionary)
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
