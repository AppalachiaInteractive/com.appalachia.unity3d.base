#region

#if UNITY_EDITOR
using UnityEditor;

#endif

#endregion

namespace Appalachia.Base.Scriptables
{
    public abstract class NestedScriptableObject<T, TP> : InternalScriptableObject<T>
        where T : NestedScriptableObject<T, TP>
        where TP : InternalScriptableObject<TP>
    {
        protected abstract string DefaultName { get; }
        public abstract void Initialize(TP parent);
#if UNITY_EDITOR
        public static T CreateNested(TP parent, bool initialize = true)
        {
            var path = AssetDatabase.GetAssetPath(parent);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            T instance = null;

            for (var index = 0; index < subAssets.Length; index++)
            {
                var subAsset = subAssets[index];
                if (subAsset is T existing)
                {
                    instance = existing;
                    break;
                }
            }

            if (instance == null)
            {
                instance = CreateInstance<T>();
                instance.name = instance.DefaultName;
                AssetDatabase.AddObjectToAsset(instance, parent);
            }

            instance.name = instance.DefaultName;

            if (initialize)
            {
                instance.Initialize(parent);
            }

            return instance;
        }
#endif
    }
}
