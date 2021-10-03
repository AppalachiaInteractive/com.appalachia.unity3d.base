using System;
using System.IO;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Appalachia.Core.Scriptables
{
    public static class ScriptableObjectFactory
    {
        private const string _PRF_PFX = nameof(ScriptableObjectFactory) + ".";
        private static readonly ProfilerMarker _PRF_CreateNew = new ProfilerMarker(_PRF_PFX + nameof(CreateNew));
        private static readonly ProfilerMarker _PRF_LoadOrCreateNew = new ProfilerMarker(_PRF_PFX + nameof(LoadOrCreateNew));

        public static T CreateNew<T>()
            where T : InternalScriptableObject<T>
        {
            using (_PRF_CreateNew.Auto())
            {
                return LoadOrCreateNew<T>($"{typeof(T).Name}_{DateTime.Now:yyyyMMdd-hhmmssfff}.asset", true, false, false);
            }
        }

        public static T LoadOrCreateNew<T>(string name)
            where T : InternalScriptableObject<T>
        {
            using (_PRF_LoadOrCreateNew.Auto())
            {
                return LoadOrCreateNew<T>(name, true, false, false);
            }
        }

        public static T LoadOrCreateNew<T>(string name, bool typeFolder, bool prependType, bool appendType)
            where T : InternalScriptableObject<T>
        {
            using (_PRF_LoadOrCreateNew.Auto())
            {
                var cleanFileName = name;
                var hasDot = name.Contains(".");
                var lastIsDot = name.EndsWith(".");

                if (lastIsDot)
                {
                    name += "asset";
                    cleanFileName = name.TrimEnd('.');
                }
                else if (!hasDot)
                {
                    name += ".asset";
                }
                else
                {
                    cleanFileName = Path.GetFileNameWithoutExtension(name);
                }

                var extension = Path.GetExtension(name);

                var t = typeof(T).Name;

                if (prependType)
                {
                    cleanFileName = $"{t}_{cleanFileName}";
                }

                if (appendType)
                {
                    cleanFileName = $"{cleanFileName}_{t}";
                }

                name = $"{cleanFileName}{extension}";

                var any = AssetDatabase.FindAssets($"t: {t} {cleanFileName}");

                for (var i = 0; i < any.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(any[i]);
                    var existingName = Path.GetFileNameWithoutExtension(path);

                    if (existingName != null && string.Equals(cleanFileName.ToLower(), existingName.ToLower()))
                    {
                        return AssetDatabase.LoadAssetAtPath<T>(path);
                    }
                }

                var instance = ScriptableObject.CreateInstance(typeof(T)) as T;
                var script = MonoScript.FromScriptableObject(instance);
                var scriptPath = AssetDatabase.GetAssetPath(script);
                var scriptFolder = Path.GetDirectoryName(scriptPath);
                var dataFolder = Path.Combine(scriptFolder, "_data");

                if (typeFolder)
                {
                    dataFolder = Path.Combine(dataFolder, t);
                }

                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                return CreateNew<T>(dataFolder, name, instance);
            }
        }

        public static T CreateNew<T>(string folder, string name)
            where T : InternalScriptableObject<T>
        {
            using (_PRF_CreateNew.Auto())
            {
                var i = ScriptableObject.CreateInstance(typeof(T)) as T;

                return CreateNew<T>(folder, name, i);
            }
        }

        public static T CreateNew<T>(string folder, string name, T i)
            where T : InternalScriptableObject<T>
        {
            using (_PRF_CreateNew.Auto())
            {
                var ext = Path.GetExtension(name);

                if (string.IsNullOrWhiteSpace(ext))
                {
                    name += ".asset";
                }

                var assetPath = Path.Combine(folder, name);

                if (File.Exists(assetPath))
                {
                    throw new AccessViolationException(assetPath);
                }

                assetPath = assetPath.Replace(Application.dataPath, "Assets");

                AssetDatabase.CreateAsset(i, assetPath);

                i.OnCreate();

                return i;
            }
        }

        public static T LoadOrCreateNew<T>(string folder, string assetName)
            where T : InternalScriptableObject<T>
        {
            using (_PRF_LoadOrCreateNew.Auto())
            {
                var extension = Path.GetExtension(assetName);

                if (string.IsNullOrWhiteSpace(extension))
                {
                    if (assetName.EndsWith("."))
                    {
                        assetName += "asset";
                    }
                    else
                    {
                        assetName += ".asset";
                    }
                }

                extension = Path.GetExtension(assetName);
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(assetName).TrimEnd('.', '-', '_', ',');
                var assetFileName = $"{nameWithoutExtension}{extension}";

                var assetPath = Path.Combine(folder, assetFileName);

                assetPath = assetPath.Replace(Application.dataPath, "Assets");

                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (asset != null)
                {
                    asset.name = nameWithoutExtension;

                    return asset;
                }

                asset = ScriptableObject.CreateInstance(typeof(T)) as T;

                asset.name = nameWithoutExtension;
                AssetDatabase.CreateAsset(asset, assetPath);

                asset.OnCreate();

                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
        }

        private static readonly ProfilerMarker _PRF_Rename = new ProfilerMarker(_PRF_PFX + nameof(Rename));
        public static void Rename<T>(T instance, string newName)
            where T : InternalScriptableObject<T>
        {
            using (_PRF_Rename.Auto())
            {
                var path = AssetDatabase.GetAssetPath(instance);
                instance.name = newName;

                AssetDatabase.RenameAsset(path, newName);
            }
        }
    }
}
