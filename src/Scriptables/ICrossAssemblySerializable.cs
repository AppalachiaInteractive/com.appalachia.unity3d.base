#region

using UnityEngine;

#endregion

namespace Appalachia.Base.Scriptables
{
    public interface ICrossAssemblySerializable
    {
        ScriptableObject GetSerializable();
    }
}
