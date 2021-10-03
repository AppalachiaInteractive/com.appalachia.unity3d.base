#region

using System;
using Unity.Profiling;

#endregion

namespace Appalachia.Core.Scriptables
{
    [Serializable]
    public abstract class SelfSavingScriptableObject<T> : InternalScriptableObject<T>
        where T : SelfSavingScriptableObject<T>
    {
#if UNITY_EDITOR
        private const string _PRF_PFX = nameof(SelfSavingScriptableObject<T>) + ".";
        private static readonly ProfilerMarker _PRF_CreateNew = new ProfilerMarker(_PRF_PFX + nameof(CreateNew));
        private static readonly ProfilerMarker _PRF_LoadOrCreateNew = new ProfilerMarker(_PRF_PFX + nameof(LoadOrCreateNew));
        private static readonly ProfilerMarker _PRF_Rename = new ProfilerMarker(_PRF_PFX + nameof(Rename));

        public static T CreateNew()
        {
            using (_PRF_CreateNew.Auto())
            {
                return ScriptableObjectFactory.CreateNew<T>();
            }
        }

        public static T LoadOrCreateNew(string name)
        {
            using (_PRF_LoadOrCreateNew.Auto())
            {
                return ScriptableObjectFactory.LoadOrCreateNew<T>(name);
            }
        }

        public static T LoadOrCreateNew(string name, bool typeFolder, bool prependType, bool appendType)
        {
            using (_PRF_LoadOrCreateNew.Auto())
            {
                return ScriptableObjectFactory.LoadOrCreateNew<T>(name, typeFolder, prependType, appendType);
            }
        }

        public static T CreateNew(string folder, string name)
        {
            using (_PRF_CreateNew.Auto())
            {
                return ScriptableObjectFactory.CreateNew<T>(folder, name);
            }
        }

        public static T CreateNew(string folder, string name, T i)
        {
            using (_PRF_CreateNew.Auto())
            {
                return ScriptableObjectFactory.CreateNew(folder, name, i);
            }
        }

        public static T LoadOrCreateNew(string folder, string assetName)
        {
            using (_PRF_LoadOrCreateNew.Auto())
            {
                return ScriptableObjectFactory.LoadOrCreateNew<T>(folder, assetName);
            }
        }

        public void Rename(string newName)
        {
            using (_PRF_Rename.Auto())
            {
                ScriptableObjectFactory.Rename(this as T, newName);
            }
        }
#endif
    }
}
