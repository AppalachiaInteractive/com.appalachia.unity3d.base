#region

using System;

#endregion

namespace Appalachia.Core.Behaviours
{
    [Serializable]
    public abstract class InternalBase<T>
        where T : InternalBase<T>
    {
    }
}
