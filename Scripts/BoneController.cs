using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace Cubeage
{
    [Serializable]
    public class BoneController : ISerializationCallbackReceiver
    {
        [SerializeReference] public Controller Controller;
        public string Name = "";
        [SerializeReference] public List<Bone> Bones = new List<Bone>();
        
        public float DefaultValue = 50;

        [SerializeField]
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (Equals(_isEnabled, value))
                    return;

                foreach(var entry in Bones.SelectMany(x => x.Properties.Values).Where(x => x.IsEnabled))
                {
                    entry.Value = value ? entry.GetValue(Value) : entry.DefaultValue;
                }
                _isEnabled = value;
            }
        }


        [SerializeField]
        private float _value = 50;

        [SerializeField]
        private Mode _mode = Mode.View;

        [SerializeField]
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (Equals(_isExpanded, value))
                    return;
                _isExpanded = value;
                if (!value)
                    Mode = Mode.View;
            }
        }
        public float Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;
                _value = value;
                if (value == 100)
                    Mode = Mode.Max;
                else if (value == 0)
                    Mode = Mode.Min;
                else
                    Mode = Mode.View;
                Update();
            }
        }

        public Mode Mode { 
            get => _mode; 
            set
            {
                if (Equals(_mode, value))
                    return;
                switch (value)
                {
                    case Mode.Max:
                        Value = 100;
                        break;
                    case Mode.Min:
                        Value = 0;
                        break;
                }
                _mode = value;
            } 
        }

        public BoneController(Controller controller, string name)
        {
            Controller = controller;
            Name = name;
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
            foreach ((var property, var boneProperty) in Bones.SelectMany(x => x.Properties).Where(x => x.Value.IsEnabled))
            {
                boneProperty.Value = boneProperty.Min + (boneProperty.Max - boneProperty.Min) * ratio;
            }
        }


        public void Add(Bone bone)
        {
            // Check Controller Part within the avatar
            if (!Controller.Avatar.GetComponentsInChildren<Transform>().Contains(bone.Part))
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

        public void Add(Transform part)
        {
            if (!Controller.ValidBones.Contains(part))
                throw new Exception("Component is not valid.");

            Add(new Bone(this, part));
        }

        public void Remove(Bone bone)
        {
            Bones.Remove(bone);
        }

        public void OnBeforeSerialize()
        {
        }


        public void OnAfterDeserialize()
        {
            // Update();
        }
    }

}