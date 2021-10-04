using UnityEngine;

namespace Appalachia.Base.Behaviours
{
    public class MasterObjectReference : SingletonMonoBehaviour<MasterObjectReference>
    {
        public Camera mainCamera;

        public GameObject mainCharacter;
    }
}
