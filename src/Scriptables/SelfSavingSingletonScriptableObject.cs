#region

using System;
using System.Diagnostics;
using System.IO;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

#endregion

namespace Appalachia.Base.Scriptables
{
    [Serializable]
    public abstract class SelfSavingSingletonScriptableObject<T> : SelfSavingScriptableObject<T>,
        ICrossAssemblySerializable,
        ISingletonScriptableObject
        where T : SelfSavingSingletonScriptableObject<T>
    {
        private const string _PRF_PFX = nameof(SelfSavingSingletonScriptableObject<T>) + ".";
        private static T _instance;

        private static readonly ProfilerMarker _PRF_instance = new(_PRF_PFX + nameof(instance));

        private static readonly ProfilerMarker _PRF_GetSerializable =
            new(_PRF_PFX + nameof(GetSerializable));

        private static readonly ProfilerMarker _PRF_SetInstance =
            new(_PRF_PFX + nameof(SetInstance));

        private static readonly ProfilerMarker _PRF_OnEnable = new(_PRF_PFX + nameof(OnEnable));

        public static T instance
        {
            get
            {
                using (_PRF_instance.Auto())
                {
#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        InitializeSingletonUsage();
                    }
#endif
                    return _instance;
                }
            }
        }

        protected virtual void OnEnable()
        {
            using (_PRF_OnEnable.Auto())
            {
                _instance = this as T;

                WhenEnabled();
            }
        }

        public ScriptableObject GetSerializable()
        {
            using (_PRF_GetSerializable.Auto())
            {
                return _instance;
            }
        }

        public void SetInstance(ISingletonScriptableObject i)
        {
            using (_PRF_SetInstance.Auto())
            {
                _instance = i as T;
            }
        }

        protected virtual void WhenEnabled()
        {
        }

#if UNITY_EDITOR

        private static readonly ProfilerMarker _PRF_InitializeSingletonUsage =
            new(_PRF_PFX + nameof(InitializeSingletonUsage));

        private static void InitializeSingletonUsage()
        {
            using (_PRF_InitializeSingletonUsage.Auto())
            {
                var stack = new StackTrace();

                if (stack.FrameCount > 255)
                {
                    throw new NotSupportedException("Stack too deep!");
                }

                var searchTerm = $"t:{typeof(T).Name}";
                var guids = AssetDatabase.FindAssets(searchTerm);

                for (var index = 0; index < guids.Length; index++)
                {
                    var guid = guids[index];
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var type = AssetDatabase.GetMainAssetTypeAtPath(path);

                    if (typeof(T).IsAssignableFrom(type))
                    {
                        var i = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                        if (i == null)
                        {
                            continue;
                        }

                        var cast = i as T;

                        _instance = cast;
                        return;
                    }
                }

                _instance = CreateAndSaveSingleton();

                SingletonScriptableObjectLookup.ScanExternal();
            }
        }

        private static T CreateAndSaveSingleton()
        {
            using (_PRF_CreateAndSaveSingleton.Auto())
            {
                return CreateAndSaveSingleton(
                    $"{typeof(T).Name}_{DateTime.Now:yyyyMMdd-hhmmssfff}.asset"
                );
            }
        }

        private static readonly ProfilerMarker _PRF_CreateAndSaveSingleton =
            new(_PRF_PFX + nameof(CreateAndSaveSingleton));

        private static T CreateAndSaveSingleton(string name)
        {
            using (_PRF_CreateAndSaveSingleton.Auto())
            {
                var any = AssetDatabase.FindAssets($"t: {typeof(T).Name}");

                if (any.Length > 0)
                {
                    var target = any[0];
                    var path = AssetDatabase.GUIDToAssetPath(target);

                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }

                var inst = CreateInstance(typeof(T)) as T;
                var script = MonoScript.FromScriptableObject(inst);
                var scriptPath = AssetDatabase.GetAssetPath(script);
                var scriptFolder = Path.GetDirectoryName(scriptPath);
                var dataFolder = Path.Combine(scriptFolder, "_data");

                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                return CreateNew(dataFolder, name, inst);
            }
        }

#endif
    }
}
