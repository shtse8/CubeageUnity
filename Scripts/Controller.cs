using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;


namespace Cubeage
{
    public class Controller : MonoBehaviour
    {
        [SerializeReference] public GameObject Avatar;
        [SerializeReference] public List<BoneController> BoneControllers = new List<BoneController>();

        void Reset()
        {
            Avatar = gameObject;
        }

        public void AddController()
        {
            BoneControllers.Add(new BoneController(this, $"Controller {BoneControllers.Count + 1}"));
        }

    }

    [Serializable]
    public class BoneController 
    {
        public SerializableGuid Id = SerializableGuid.NewGuid();
        [SerializeReference] public Controller Controller;
        public string Name = "";
        [SerializeReference] public List<Bone> Bones = new List<Bone>();
        public bool isExpanded = false;
        public float Value = 50;
        public float DefaultValue = 50;
        public Mode Mode = Mode.View;

        public BoneController(Controller controller, string name)
        {
            Controller = controller;
            Name = name;
            Debug.Log(Id);
        }

        void RemoveInvalidBones()
        {
            foreach (var x in Bones.Where(x => !x.IsValid()).ToArray())
            {
                Remove(x);
            }
        }

        public IEnumerable<Bone> GetValidBones()
        {
            RemoveInvalidBones();
            return Bones.ToArray();
        }

        public void Update()
        {
            var ratio = Value / 100;
            foreach (var bone in Bones)
            {
                foreach ((var property, var boneProperty) in bone.Properties.Where(x => x.Value.IsEnabled))
                {
                    float value = boneProperty.Min + (boneProperty.Max - boneProperty.Min) * ratio;
                    bone.Transform(property, value);
                }
            }
        }

        public void Add(Bone bone)
        {
            // Check Controller Part within the avatar
            if (!Controller.Avatar.GetComponentsInChildren<ControllerPart>().Contains(bone.Part))
            {
                throw new Exception("This part doesn't belong to this avatar.");
            }
            // check duplicated part in the controller
            else if (Bones.Select(x => x.Part).Contains(bone.Part))
            {
                throw new Exception("Duplicated part.");
            }
            else
            {
                Bones.Add(bone);
            }
        }

        public void SetDefault()
        {
            DefaultValue = Value;
        }

        public void SetValue(float value, bool noUpdate = false)
        {
            Value = value;
            if (!noUpdate)
                Update();
        }

        public void Reset()
        {
            SetValue(DefaultValue);
        }

        public void Add(ControllerPart part)
        {
            Add(new Bone(this, part));
        }

        public void Remove(Bone bone)
        {
            Bones.Remove(bone);
        }

    }

    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        public static SerializableGuid Empty = new SerializableGuid(Guid.Empty);

        public string value;
        // [SerializeField]
        // public byte[] values;

        public SerializableGuid(Guid guid)
        {
            // values = guid.ToByteArray();
            value = guid.ToString();
        }

        public bool Equals(SerializableGuid other)
        {
            // return values.SequenceEqual(other.values);
            return value.Equals(other.value);
        }

        public static SerializableGuid NewGuid()
        {
            return new SerializableGuid(Guid.NewGuid());
        }

        public override string ToString() {
            // return new Guid(values).ToString();
            return value;
        }
    }

    public enum Mode
    {
        View,
        Min,
        Max
    }

}