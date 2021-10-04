#region

using System;
using System.Collections.Generic;
using Appalachia.Editing.Attributes;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Base.Scriptables
{
    [Serializable]
    public abstract class SelfNamingSavingAndIdentifyingScriptableObject<T> : SelfSavingAndIdentifyingScriptableObject<T>,
                                                                              IComparable<SelfNamingSavingAndIdentifyingScriptableObject<T>>,
                                                                              IComparable
        where T : SelfNamingSavingAndIdentifyingScriptableObject<T>
    {
        [FoldoutGroup("Metadata")]
        [OnValueChanged(nameof(UpdateName))]
        [DelayedProperty, PropertyOrder(-1000), SmartLabel]
        public string profileName;

        int IComparable.CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is SelfNamingSavingAndIdentifyingScriptableObject<T> other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(SelfNamingSavingAndIdentifyingScriptableObject<T>)}");
        }

        public int CompareTo(SelfNamingSavingAndIdentifyingScriptableObject<T> other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var selfSavingAndIdentifyingScriptableObjectComparison = base.CompareTo(other);
            if (selfSavingAndIdentifyingScriptableObjectComparison != 0)
            {
                return selfSavingAndIdentifyingScriptableObjectComparison;
            }

            return string.Compare(profileName, other.profileName, StringComparison.Ordinal);
        }

        public void UpdateName()
        {
            if (name != profileName)
            {
                Rename(profileName);

                UpdateAllIDs();
            }
        }

        protected virtual void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                profileName = name;
                UpdateName();
            }
        }

        protected override void OnUpateAllIDs()
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                Rename(profileName);
            }
        }

        public static bool operator <(SelfNamingSavingAndIdentifyingScriptableObject<T> left, SelfNamingSavingAndIdentifyingScriptableObject<T> right)
        {
            return Comparer<SelfNamingSavingAndIdentifyingScriptableObject<T>>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(SelfNamingSavingAndIdentifyingScriptableObject<T> left, SelfNamingSavingAndIdentifyingScriptableObject<T> right)
        {
            return Comparer<SelfNamingSavingAndIdentifyingScriptableObject<T>>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(
            SelfNamingSavingAndIdentifyingScriptableObject<T> left,
            SelfNamingSavingAndIdentifyingScriptableObject<T> right)
        {
            return Comparer<SelfNamingSavingAndIdentifyingScriptableObject<T>>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(
            SelfNamingSavingAndIdentifyingScriptableObject<T> left,
            SelfNamingSavingAndIdentifyingScriptableObject<T> right)
        {
            return Comparer<SelfNamingSavingAndIdentifyingScriptableObject<T>>.Default.Compare(left, right) >= 0;
        }
    }
}
