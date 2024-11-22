#if UNITY_EDITOR
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Utility
{
    public static class EditorResourcesManager //sync with assets provider service?
    {
        private const string RESOURCES_PATH = "Resources/";
        
        public static void CreateJson(string fileName, string content)
        {
            var directoryPath = Path.Combine(Application.dataPath + "/"+ RESOURCES_PATH + Constants.ResourcesPaths.DATA);
            if (Directory.Exists(directoryPath)==false)
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }

            var filePath = Path.Combine(directoryPath, $"{fileName}.json");

            File.WriteAllText(filePath, content);

            AssetDatabase.Refresh();
        }
        
        public static void CreateJson<T>(string content)
        {
            CreateJson(typeof(T).Name, content);
        }
        
        public static T LoadJsonDataAsset<T>() where T: class
        {
            var assetName = typeof(T).Name;
            var path = Path.Combine(Constants.ResourcesPaths.DATA + assetName);
            var json = Resources.Load<TextAsset>(path);
        
            if (json == null)
            {
                Debug.LogError($"Path not found: {assetName}");
                return null;
            }
            
            T levelData = JsonConvert.DeserializeObject<T>(json.text);
            
            if (levelData == null)
            {
                Debug.LogError($"Json cant be serialized: {assetName}");
            }
        
            return levelData;
        }
    }
}
#endif
