using System;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Interfaces;
using Appalachia.Core.Editing.AssetDB;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Core.Scriptables
{
    [Serializable]
    public abstract class ScriptableObjectLookupCollection<T, TI, TKey, TValue, TKeyList, TValueList> : SelfSavingSingletonScriptableObject<T>
        where T : ScriptableObjectLookupCollection<T, TI, TKey, TValue, TKeyList, TValueList>
        where TI : AppaLookup<TKey, TValue, TKeyList, TValueList>, new()
        where TKeyList : AppaList<TKey>, new()
        where TValueList : AppaList<TValue>, new()
        where TValue : InternalScriptableObject<TValue>
    {
        private const string _PRF_PFX = nameof(ScriptableObjectLookupCollection<T, TI, TKey, TValue, TKeyList, TValueList>) + ".";
        
        [SerializeField, InlineProperty, HideLabel, LabelWidth(0), ShowInInspector]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true, NumberOfItemsPerPage = 3)]
        protected TI _items;

        private static readonly ProfilerMarker _PRF_Items = new ProfilerMarker(_PRF_PFX + nameof(Items));
        public IAppaLookupSafeUpdates<TKey, TValue, TValueList> Items
        {
            get
            {
                using (_PRF_Items.Auto())
                {
#if UNITY_EDITOR
                    PopulateItems();
#endif

                    return _items;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_WhenEnabled = new ProfilerMarker(_PRF_PFX + nameof(WhenEnabled));
        protected override void WhenEnabled()
        {
            using (_PRF_WhenEnabled.Auto())
            {
#if UNITY_EDITOR
                PopulateItems();
#endif
            }
        }
     
#if UNITY_EDITOR
        [NonSerialized] private bool _initialized;
        private static readonly ProfilerMarker _PRF_PopulateItems = new ProfilerMarker(_PRF_PFX + nameof(PopulateItems));
        [ButtonGroup]
        private void PopulateItems()
        {
            using (_PRF_PopulateItems.Auto())
            {
                if (_items == null)
                {
                    _items = new TI();
                    SetDirty();
                }

                if (_initialized)
                {
                    return;
                }
                
                _initialized = true;
                _items.SetDirtyAction(SetDirty);

                var anyNull = false;

                for (var i = 0; i < _items.Count; i++)
                {
                    var item = _items.at[i];
                    var key = _items.keysAt[i];

                    if (item == null || key == null)
                    {
                        anyNull = true;
                        break;
                    }
                }
                
                var assets = AssetDatabaseHelper.FindAssetsList<TValue>();

                if (anyNull || assets.Count != _items.Count)
                {
                    _items.Clear();
                    
                    for (var i = 0; i < assets.Count; i++)
                    {
                        var value = assets[i];
                        var key = GetUniqueKeyFromValue(value);
                        _items.Add(key, value);
                    }
                }
            }
        }

        [ButtonGroup]
        private void RepopulateItems()
        {
            _initialized = false;
            PopulateItems();
        }
#endif

        private static readonly ProfilerMarker _PRF_RemoveInvalid = new ProfilerMarker(_PRF_PFX + nameof(RemoveInvalid));
        public void RemoveInvalid()
        {
            using (_PRF_RemoveInvalid.Auto())
            {
                for (var i = _items.Count - 1; i >= 0; i--)
                {
                    var item = _items.at[i];

                    if (!IsValid(item))
                    {
                        _items.RemoveAt(i);
                    }
                }

                SetDirty();
            }
        }

        protected abstract TKey GetUniqueKeyFromValue(TValue value);

        // ReSharper disable once UnusedParameter.Global
        protected virtual bool IsValid(TValue element)
        {
            return true;
        }

        private static readonly ProfilerMarker _PRF_DoForAll = new ProfilerMarker(_PRF_PFX + nameof(DoForAll));
        public void DoForAll(Action<TValue> action)
        {
            using (_PRF_DoForAll.Auto())
            {
                var count = Items.Count;

                for (var i = 0; i < count; i++)
                {
                    action(Items.at[i]);
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DoForAllIf = new ProfilerMarker(_PRF_PFX + nameof(DoForAllIf));
        public void DoForAllIf(Predicate<TValue> doIf, Action<TValue> action)
        {
            using (_PRF_DoForAllIf.Auto())
            {
                var count = Items.Count;

                for (var i = 0; i < count; i++)
                {
                    var set = Items.at[i];

                    if (doIf(set))
                    {
                        action(set);
                    }
                }
            }
        }

        public TValue GetOrLoadOrCreateNew(TKey key, string name)
        {
            var items = Items;

            if (items.ContainsKey(key))
            {
                return items.Get(key);
            }

            var instance = ScriptableObjectFactory.LoadOrCreateNew<TValue>(name);

            items.Add(key, instance);

            return instance;
        }

        public virtual void OnDisable()
        {
        }
    }
}
