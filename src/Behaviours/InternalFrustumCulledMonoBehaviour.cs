using Unity.Profiling;

namespace Appalachia.Core.Behaviours
{
    public class InternalFrustumCulledMonoBehaviour<T>: InternalMonoBehaviour
        where T : InternalFrustumCulledMonoBehaviour<T>
    {
        private const string _PRF_PFX = nameof(InternalFrustumCulledMonoBehaviour<T>) + ".";
        
        private static readonly ProfilerMarker _PRF_OnBecameVisible = new ProfilerMarker(_PRF_PFX + nameof(OnBecameVisible));
        private void OnBecameVisible()
        {
            using (_PRF_OnBecameVisible.Auto())
            {
                 BeforeVisible();
                enabled = true;
            }
        }

        private static readonly ProfilerMarker _PRF_OnBecameInvisible = new ProfilerMarker(_PRF_PFX + nameof(OnBecameInvisible));
        private void OnBecameInvisible()
        {
            using (_PRF_OnBecameInvisible.Auto())
            {
                BeforeInvisible();
                enabled = false;
            }
        }

        protected virtual void BeforeVisible()
        {
            
        }

        protected virtual void BeforeInvisible()
        {
            
        }
    }
}
