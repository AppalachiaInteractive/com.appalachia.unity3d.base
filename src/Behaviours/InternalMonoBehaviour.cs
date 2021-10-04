#region

using System;
using System.Linq;
using Appalachia.Core.Scene.Framing;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Appalachia.Base.Behaviours
{
    [Serializable]
    public class InternalMonoBehaviour : MonoBehaviour
    {
        private Bounds ___renderingBounds;
        private Transform ___transform;

        protected Transform _transform
        {
            get
            {
                if (___transform == null)
                {
                    ___transform = transform;
                }

                return ___transform;
            }
        }

        public Bounds renderingBounds
        {
            get
            {
                if (___renderingBounds == default)
                {
                    ___renderingBounds = gameObject.GetRenderingBounds();
                }

                return ___renderingBounds;
            }
        }

        protected void RecalculateBounds()
        {
            ___renderingBounds = default;
        }

        protected float3 LocalToWorldPoint(float3 point)
        {
            return _transform.TransformPoint(point);
        }

        protected float3 WorldToLocalPoint(float3 point)
        {
            return _transform.InverseTransformPoint(point);
        }

        protected float3 LocalToWorldDirection(float3 direction)
        {
            return _transform.TransformDirection(direction);
        }

        protected float3 WorldToLocalDirection(float3 direction)
        {
            return _transform.InverseTransformDirection(direction);
        }

        protected float3 LocalToWorldVector(float3 vector)
        {
            return _transform.TransformVector(vector);
        }

        protected float3 WorldToLocalVector(float3 vector)
        {
            return _transform.InverseTransformVector(vector);
        }

#if UNITY_EDITOR

        protected void SetDirty()
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        [Button(ButtonSizes.Small)]
        [HorizontalGroup("base_A")]
        [PropertyOrder(-1000)]
        private void SelectAllInScene()
        {
            var type = GetType();

            var instances = FindObjectsOfType(type);

            Selection.objects = instances;
        }

        [Button(ButtonSizes.Small)]
        [HorizontalGroup("base_A")]
        [PropertyOrder(-1000)]
        private void SelectObjectsInScene()
        {
            var type = GetType();

            // ReSharper disable once CoVariantArrayConversion
            Object[] instances = FindObjectsOfType(type)
                                .Select(
                                     o =>
                                     {
                                         if (o is Component c)
                                         {
                                             return c.gameObject;
                                         }

                                         return null;
                                     }
                                 )
                                .ToArray();

            Selection.objects = instances;
        }

        public void Frame(bool recalculateBounds = false, bool adjustAngle = true)
        {
            if (recalculateBounds)
            {
                RecalculateBounds();

                var filters = GetComponentsInChildren<MeshFilter>();

                for (var i = 0; i < filters.Length; i++)
                {
                    var mf = filters[i];

                    if (mf != null)
                    {
                        var mesh = mf.sharedMesh;
                        mesh.RecalculateBounds();
                        mesh.UploadMeshData(false);
                    }
                }
            }

            gameObject.Frame(renderingBounds, adjustAngle);
        }
#endif
    }
}
