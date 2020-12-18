using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Linq;
using System;


namespace Cubeage
{
    [Serializable]
    public struct TransformData
    {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Vector3 localEulerAngles;
    }

    public class Controller : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeReference]
        [SerializeField]
        private GameObject _avatar;

        public GameObject Avatar
        {
            get => _avatar;
            set {
                _avatar = value;
                UpdateValidBones();
            }
        }

        public SerializableDictionary<Transform, TransformData> ValidBones = new SerializableDictionary<Transform, TransformData>();

        [SerializeReference] public List<BoneController> BoneControllers = new List<BoneController>();

        void Reset()
        {
            Avatar = gameObject;
        }

        public bool HasAnimator()
        {
            return gameObject.GetComponent<Animator>();
            // foreach(var x  in controller.BoneControllers[0].Bones[0].Properties)
            // {
            //     Debug.Log(x.Key, x.Value);
            // }
            // Animator lAnimator = controller.gameObject.GetComponent<Animator>();
            // Debug.Log(lAnimator);
            // 
            // Transform lBoneTransform = lAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            // 
            // Debug.Log(lBoneTransform);
        }

        public bool TryGetTargetTransform(Transform transform, out Transform target)
        {
            target = null;
            for(var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (Equals(child.name + "_bc", transform.name))
                {
                    target = child;
                    return true;
                }
            }
            return false;
        }

        void UpdateValidBones()
        {
            ValidBones.Clear();
            foreach (var transform in Avatar.GetComponentsInChildren<Transform>().Where(x => IsBone(x)))
            {
                var data = GetData(transform);
                ValidBones.Add(transform, data);
            }
        }

        public void AddController()
        {
            BoneControllers.Add(new BoneController(this, $"Controller {BoneControllers.Count + 1}"));
        }

        public static bool IsBone(Component component)
        {
            return component.name.EndsWith("_bc");
        }

        public void ResetBones()
        {
            var dict = new Dictionary<BoneController, bool>();
            foreach(var controller in BoneControllers)
            {
                dict.Add(controller, controller.IsEnabled);
                controller.IsEnabled = false;
            }

            // Reset transform.
            foreach((var transform, var data) in ValidBones)
            {
                transform.localPosition = data.localPosition;
                transform.localScale = data.localScale;
                transform.localEulerAngles = data.localEulerAngles;
            }

            foreach ((var controller, var isEnabled) in dict)
            {
                controller.IsEnabled = isEnabled;
            }
        }

        public void SetToDefault()
        {
            foreach(var controller in BoneControllers)
            {
                controller.SetToDefault();
            }
        }

        TransformData GetData(Transform transform)
        {
            return new TransformData
            {
                localPosition = transform.localPosition,
                localEulerAngles = transform.localEulerAngles,
                localScale = transform.localScale
            };
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }
    }
}