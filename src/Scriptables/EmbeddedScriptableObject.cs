#region

using System;
using UnityEditor;
using UnityEngine;

#endregion

#if UNITY_EDITOR

#endif

namespace Appalachia.Core.Scriptables
{
    [Serializable]
    public abstract class EmbeddedScriptableObject<T> : InternalScriptableObject<T>
        where T : EmbeddedScriptableObject<T>
    {
#if UNITY_EDITOR
        public static TC CreateAndSaveInExisting<TC>(GameObject mainAsset)
            where TC : InternalScriptableObject<TC>
        {
            var assetName = $"{typeof(TC).Name}_{DateTime.Now:yyyyMMdd-hhmmssfff}.asset";

            return CreateAndSaveInExisting<TC>(mainAsset, assetName);
        }

        public static TC CreateAndSaveInExisting<TC>(GameObject mainAsset, string assetName)
            where TC : InternalScriptableObject<TC>
        {
            var path = AssetDatabase.GetAssetPath(mainAsset);

            if (path == null)
            {
                return null;
            }

            return CreateAndSaveInExisting<TC>(path, assetName);
        }

        public static TC CreateAndSaveInExisting<TC>(string assetPath, string assetName)
            where TC : InternalScriptableObject<TC>
        {
            var instance = (TC) CreateInstance(typeof(TC));
            instance.name = assetName;

            AssetDatabase.AddObjectToAsset(instance, assetPath);

            return instance;
        }
#endif
    }
}
