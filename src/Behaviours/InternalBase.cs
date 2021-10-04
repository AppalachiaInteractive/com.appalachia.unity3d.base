#region

using System;

#endregion

namespace Appalachia.Base.Behaviours
{
    [Serializable]
    public abstract class InternalBase<T>
        where T : InternalBase<T>
    {
    }
}
