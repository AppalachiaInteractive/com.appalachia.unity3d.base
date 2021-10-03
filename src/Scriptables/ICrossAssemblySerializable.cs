#region

using UnityEngine;

#endregion

namespace Appalachia.Core.Scriptables
{
    public interface ICrossAssemblySerializable
    {
        ScriptableObject GetSerializable();
    }
}
