#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Services.AssetsService;
using UnityEditor;
using UnityEngine;

namespace Utility
{
    public static class EditorResourcesManager //sync with assets provider service?
    {
        private const string RESOURCES_PATH = "Resources/";
        
        public static void CreateJson(string fileName, string content)
        {
            var directoryPath = Path.Combine(Application.dataPath + "/"+ RESOURCES_PATH, Constants.ResourcesPaths.DATA);
            if (Directory.Exists(directoryPath)==false)
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }

            var filePath = Path.Combine(directoryPath, $"{fileName}.json");

            File.WriteAllText(filePath, content);

            AssetDatabase.Refresh();
        }

        public static T LoadJsonDataAsset<T>(string dataName) where T: class
        {
            var path = Path.Combine(Constants.ResourcesPaths.DATA, dataName);
            
            var json = Resources.Load<TextAsset>(path);

            if (json == null)
            {
                Debug.LogError($"Path not found: {dataName}");
                return null;
            }
            
            T jsonData = JsonConvert.DeserializeObject<T>(json.text);
            Resources.UnloadAsset(json);

            if (jsonData == null)
            {
                Debug.LogError($"Json cant be serialized: {dataName}");
            }
            
            return jsonData;
        }
    }
}
#endif
