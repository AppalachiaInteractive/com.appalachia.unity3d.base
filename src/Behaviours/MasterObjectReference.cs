using UnityEngine;

namespace Appalachia.Core.Behaviours
{
    public class MasterObjectReference : SingletonMonoBehaviour<MasterObjectReference>
    {
        public Camera mainCamera;

        public GameObject mainCharacter;
    }
}
