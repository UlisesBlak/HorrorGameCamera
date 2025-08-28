using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SimpleFolderIcon.Editor
{
    public class IconDictionaryCreator : AssetPostprocessor
    {
        private static int _pathIndex = -1;
        private static readonly string[] PossiblePaths = {
            "Assets/SimpleFolderIcon/Icons",
            "Assets/Plugins/SimpleFolderIcon/Icons",
            "Assets/Resources/SimpleFolderIcon/Icons" // Don't know, didn't think any further than this
        };
        internal static Dictionary<string, Texture> IconDictionary;
        
        private static string GetIconsPath()
        {
            if (_pathIndex >= 0) { // If the path index is set, return the corresponding path directly 
                string fullPath = Path.Combine(Application.dataPath, PossiblePaths[_pathIndex].Substring("Assets/".Length));
                if (Directory.Exists(fullPath))
                    return PossiblePaths[_pathIndex];
            }
            for (int i = 0; i < PossiblePaths.Length; i++) { // Search for the first existing path
                string fullPath = Path.Combine(Application.dataPath, PossiblePaths[i].Substring("Assets/".Length));
                if (!Directory.Exists(fullPath)) continue;
                _pathIndex = i;
                return PossiblePaths[i];
            }
            // If no path exists, return the first one and reset the index
            _pathIndex = 0;
            return PossiblePaths[0];
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!ContainsIconAsset(importedAssets) &&
                !ContainsIconAsset(deletedAssets) &&
                !ContainsIconAsset(movedAssets) &&
                !ContainsIconAsset(movedFromAssetPaths))
            { return; }
            BuildDictionary();
        }

        private static bool ContainsIconAsset(string[] assets)
        {
            if (_pathIndex >= 0) {
                string activePath = PossiblePaths[_pathIndex];
                foreach (string str in assets) {
                    string dir = ReplaceSeparatorChar(Path.GetDirectoryName(str));
                    if (dir == activePath) return true;
                }
                return false;
            }
            foreach (string str in assets) {
                string dir = ReplaceSeparatorChar(Path.GetDirectoryName(str));
                foreach (var path in PossiblePaths) {
                    if (dir == path) return true;
                }
            }
            return false;
        }

        private static string ReplaceSeparatorChar(string path)
        {
            return path.Replace("\\", "/");
        }

        internal static void BuildDictionary()
        {
            var dictionary = new Dictionary<string, Texture>();
            string iconsPath = GetIconsPath();
            var dir = new DirectoryInfo(Path.Combine(Application.dataPath, iconsPath.Substring("Assets/".Length)));
            FileInfo[] info = dir.GetFiles("*.png");
            foreach(FileInfo f in info) {
                var texture = (Texture)AssetDatabase.LoadAssetAtPath($"{iconsPath}/{f.Name}", typeof(Texture2D));
                dictionary.Add(Path.GetFileNameWithoutExtension(f.Name),texture);
            }
        
            FileInfo[] infoSo = dir.GetFiles("*.asset");
            foreach (FileInfo f in infoSo) {
                var folderIconSo = (FolderIconScrObj)AssetDatabase.LoadAssetAtPath($"{iconsPath}/{f.Name}", typeof(FolderIconScrObj));
                if (folderIconSo != null)  { //Can be inverted to reduce nesting, but != null is 'allegedly' more performant
                    var texture = (Texture)folderIconSo.icon;
                    foreach (string folderName in folderIconSo.folderNames) {
                        if (folderName != null) {
                            dictionary.TryAdd(folderName, texture);
                        }
                    }
                }
            }
            IconDictionary = dictionary;
        }
    }
}
