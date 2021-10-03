#region

using Appalachia.Core.Globals.Shading;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Core.Behaviours
{
    public abstract class InternalInstancedMonoBehaviour : InternalMonoBehaviour
    {
        protected bool checkedForRenderers;

        [SerializeField, HideInInspector]
        private MeshRenderer[] _meshRenderers;

        [SerializeField, HideInInspector]
        private MaterialPropertyBlock[] _materialPropertyBlocks;

        protected MeshRenderer[] meshRenderers
        {
            get
            {
                if ((_meshRenderers == null) || ((_meshRenderers.Length == 0) && !checkedForRenderers))
                {
                    _meshRenderers = GetComponentsInChildren<MeshRenderer>();
                    checkedForRenderers = true;
                }

                return _meshRenderers;
            }
        }

        protected MaterialPropertyBlock[] materialPropertyBlocks
        {
            get
            {
                var mr = meshRenderers;

                if ((mr != null) && ((_materialPropertyBlocks == null) || (_materialPropertyBlocks.Length != meshRenderers.Length)))
                {
                    _materialPropertyBlocks = new MaterialPropertyBlock[meshRenderers.Length];

                    for (var i = 0; i < _materialPropertyBlocks.Length; i++)
                    {
                        _materialPropertyBlocks[i] = new MaterialPropertyBlock();
                        mr[i].GetPropertyBlock(_materialPropertyBlocks[i]);
                    }
                }

                return _materialPropertyBlocks;
            }
        }

        [Button]
        public void UpdateAllInstancedProperties()
        {
            try
            {
                var mrs = meshRenderers;
                var bs = materialPropertyBlocks;

                for (var i = 0; i < bs.Length; i++)
                {
                    var b = bs[i];
                    var mr = mrs[i];

                    var mats = mr.sharedMaterials;

                    if (mats.Length == 0)
                    {
                        continue;
                    }

                    var mat = mats[0];

                    var shader = mat.shader;

                    GSPL.Include(shader);

                    if (b == null)
                    {
                        continue;
                    }

                    UpdateInstancedProperties(b, mat);

                    mr.SetPropertyBlock(b);
                }
            }
            catch (MissingReferenceException)
            {
                _meshRenderers = null;
                _materialPropertyBlocks = null;
            }
        }


        protected abstract void UpdateInstancedProperties(MaterialPropertyBlock block, Material m);
    }
}
