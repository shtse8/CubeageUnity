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