#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Appalachia.Core.Attributes;
using Appalachia.Core.Extensions;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

#if ODIN_INSPECTOR

#endif

#endregion

#if UNITY_EDITOR

#endif

namespace Appalachia.Base.Scriptables
{
    [Critical]
    [Serializable]
    public class
        SingletonScriptableObjectLookup : InternalScriptableObject<SingletonScriptableObjectLookup>
    {
        private const string _PRF_PFX = nameof(SingletonScriptableObjectLookup) + ".";
        private static readonly ProfilerMarker _PRF_Awake = new(_PRF_PFX + "Awake");
        private static readonly ProfilerMarker _PRF_Start = new(_PRF_PFX + "Start");
        private static readonly ProfilerMarker _PRF_OnEnable = new(_PRF_PFX + "OnEnable");
        private static readonly ProfilerMarker _PRF_Update = new(_PRF_PFX + "Update");
        private static readonly ProfilerMarker _PRF_LateUpdate = new(_PRF_PFX + "LateUpdate");
        private static readonly ProfilerMarker _PRF_OnDisable = new(_PRF_PFX + "OnDisable");
        private static readonly ProfilerMarker _PRF_OnDestroy = new(_PRF_PFX + "OnDestroy");

        public List<ScriptableObject> singletons = new();
        private Dictionary<Assembly, string> _assemblyNames;

#if UNITY_EDITOR

        public List<AssemblyDefinitionAsset> excludedAssemblies = new();

        private void OnEnable()
        {
            using (_PRF_OnEnable.Auto())
            {
                Scan();
            }
        }

        private static readonly ProfilerMarker _PRF_Scan = new(_PRF_PFX + nameof(Scan));

        private static readonly ProfilerMarker _PRF_Scan_Search =
            new(_PRF_PFX + nameof(Scan) + ".Search");

        private static readonly ProfilerMarker _PRF_Scan_AssemblyLookup =
            new(_PRF_PFX + nameof(Scan) + ".AssemblyLookup");

        private static readonly ProfilerMarker _PRF_Scan_CheckExcluded =
            new(_PRF_PFX + nameof(Scan) + ".CheckExcluded");

        private static readonly ProfilerMarker _PRF_Scan_LoadInstance =
            new(_PRF_PFX + nameof(Scan) + ".LoadInstance");

        private static readonly ProfilerMarker _PRF_Scan_AddToList =
            new(_PRF_PFX + nameof(Scan) + ".AddToList");

        private static readonly ProfilerMarker _PRF_Scan_SetInstance =
            new(_PRF_PFX + nameof(Scan) + ".SetInstance");

        private static readonly ProfilerMarker _PRF_Scan_SetDirty =
            new(_PRF_PFX + nameof(Scan) + ".SetDirty");

        private static readonly ProfilerMarker _PRF_Scan_AssemblyCheck =
            new(_PRF_PFX + nameof(Scan) + ".AssemblyCheck");

#if ODIN_INSPECTOR
        [Button]
#endif
        private void Scan()
        {
            using (_PRF_Scan.Auto())
            {
                if (singletons == null)
                {
                    singletons = new List<ScriptableObject>();
                }

                singletons.Clear();

                var genericType = typeof(SelfSavingSingletonScriptableObject<>);

                HashSet<string> excludedAssemblyLookup;
                string[] guids;

                using (_PRF_Scan_AssemblyLookup.Auto())
                {
                    excludedAssemblyLookup = excludedAssemblies.Select(ea => ea.name).ToHashSet2();
                }

                using (_PRF_Scan_Search.Auto())
                {
                    guids = AssetDatabase.FindAssets("t: ScriptableObject");
                }

                for (var index = 0; index < guids.Length; index++)
                {
                    var guid = guids[index];
                    var path = AssetDatabase.GUIDToAssetPath(guid);

                    Type type;

                    using (_PRF_Scan_CheckExcluded.Auto())
                    {
                        if (string.IsNullOrWhiteSpace(path))
                        {
                            continue;
                        }

                        type = AssetDatabase.GetMainAssetTypeAtPath(path);

                        if (type == null)
                        {
                            continue;
                        }

                        var assembly = type.Assembly;

                        if (_assemblyNames == null)
                        {
                            _assemblyNames = new Dictionary<Assembly, string>();
                        }

                        using (_PRF_Scan_AssemblyCheck.Auto())
                        {
                            string assemblyName;

                            if (_assemblyNames.ContainsKey(assembly))
                            {
                                assemblyName = _assemblyNames[assembly];
                            }
                            else
                            {
                                assemblyName = assembly.GetName().Name;
                                _assemblyNames.Add(assembly, assemblyName);
                            }

                            if (excludedAssemblyLookup.Contains(assemblyName))
                            {
                                continue;
                            }
                        }
                    }

                    if (IsAssignableToGenericType(type, genericType))
                    {
                        ScriptableObject instance;

                        using (_PRF_Scan_LoadInstance.Auto())
                        {
                            instance = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                        }

                        if (instance == null)
                        {
                            continue;
                        }

                        using (_PRF_Scan_AddToList.Auto())
                        {
                            singletons.Add(instance);
                        }

                        /*var cast = instance as ISingletonScriptableObject;
                        
                        using (_PRF_Scan_SetInstance.Auto())
                        {
                            cast.SetInstance(cast);
                        }*/
                    }
                }

                EditorUtility.SetDirty(this);
            }
        }

        private static readonly ProfilerMarker _PRF_IsAssignableToGenericType =
            new(_PRF_PFX + nameof(IsAssignableToGenericType));

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            using (_PRF_IsAssignableToGenericType.Auto())
            {
                var interfaceTypes = givenType.GetInterfaces();

                for (var index = 0; index < interfaceTypes.Length; index++)
                {
                    var it = interfaceTypes[index];
                    if (it.IsGenericType && (it.GetGenericTypeDefinition() == genericType))
                    {
                        return true;
                    }
                }

                if (givenType.IsGenericType &&
                    (givenType.GetGenericTypeDefinition() == genericType))
                {
                    return true;
                }

                var baseType = givenType.BaseType;
                if (baseType == null)
                {
                    return false;
                }

                var result = IsAssignableToGenericType(baseType, genericType);

                return result;
            }
        }

        private static readonly ProfilerMarker _PRF_ScanExternal =
            new(_PRF_PFX + nameof(ScanExternal));

        public static void ScanExternal()
        {
            using (_PRF_ScanExternal.Auto())
            {
                var path = AssetDatabase.FindAssets("t: SingletonScriptableObjectLookup")
                                        .Select(g => AssetDatabase.GUIDToAssetPath(g))
                                        .FirstOrDefault();

                var instance = AssetDatabase.LoadAssetAtPath<SingletonScriptableObjectLookup>(path);
                instance.Scan();
            }
        }

#endif
    }
}
