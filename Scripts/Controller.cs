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

    [AddComponentMenu("Cubeage/Avatar Controller")]
    public class Controller : MonoBehaviour
    {
        
        public const string POSTFIX = "_ctrl";

        #region Avatar
        [SerializeReference]
        [SerializeField]
        private GameObject _avatar;

        public GameObject Avatar
        {
            get => _avatar;
            set
            {
                if (Equals(_avatar, value))
                    return;

                _avatar = value;
                UpdateValidBones();
            }
        }
        #endregion

        public Component RecordTarget => this;


        #region isEnabled
        [SerializeField]
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (Equals(_isEnabled, value))
                    return;

                _isEnabled = value;

                // Update Entries
                foreach (var entry in _boneControllers.SelectMany(x => x.Bones).SelectMany(x => x.Properties.Values).Where(x => x.IsEnabled))
                {
                    entry.Update();
                }
            }
        }

        #endregion

        #region ValidBones
        [SerializeField]
        private SerializableDictionary<Transform, TransformData> _validBones = new SerializableDictionary<Transform, TransformData>();
        public IList<Transform> ValidBones => _validBones.Keys.ToArray();
        #endregion

        #region BoneControllers
        [SerializeField]
        [SerializeReference]
        private List<BoneController> _boneControllers = new List<BoneController>();
        public IList<BoneController> BoneControllers => _boneControllers.ToArray();
        #endregion

        void Reset()
        {
            Avatar = gameObject;
        }

        #region private methods
        void UpdateValidBones()
        {
            _validBones.Clear();
            foreach (var transform in Avatar.GetComponentsInChildren<Transform>().Where(x => IsBone(x)))
            {
                var data = GetData(transform);
                _validBones.Add(transform, data);
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

        #endregion

        #region public methods
        public bool HasAnimator()
        {
            return _avatar.GetComponent<Animator>();
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
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (Equals(child.name + POSTFIX, transform.name))
                {
                    target = child;
                    return true;
                }
            }
            return false;
        }

        public void Remove(BoneController controller)
        {
            Undo.RecordObject(RecordTarget, "Remove Controller");
            _boneControllers.Remove(controller);
        }

        public void AddController()
        {
            Undo.RecordObject(RecordTarget, "Add Controller");
            _boneControllers.Add(new BoneController(this, $"Controller {_boneControllers.Count + 1}"));
        }

        public static bool IsBone(Component component)
        {
            return component.name.EndsWith(POSTFIX);
        }

        public void ResetBones()
        {
            Undo.RecordObject(RecordTarget, "Reset All Bones");

            IsEnabled = false;
            // Reset transform.
            foreach ((var transform, var data) in _validBones)
            {
                transform.localPosition = data.localPosition;
                transform.localScale = data.localScale;
                transform.localEulerAngles = data.localEulerAngles;
            }
            IsEnabled = true;
        }

        public void SetToDefault()
        {
            Undo.RecordObject(RecordTarget, "Set All Controller To Default");
            foreach (var controller in _boneControllers)
            {
                controller.SetToDefault();
            }
        }
        public bool IsValidBone(Transform bone)
        {
            return _validBones.Contains(bone);
        }
        #endregion
    }

}