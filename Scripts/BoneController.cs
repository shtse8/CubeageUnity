using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace Cubeage
{
    [Serializable]
    public class BoneController
    {
        [SerializeField]
        [SerializeReference]
        protected Controller _controller;
        public Controller Controller => _controller;

        [SerializeField]
        private  string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (Equals(_name, value))
                    return;
                Undo.RecordObject(_controller.RecordTarget, "Change Controller Name");
                _name = value;
            }
        }
        [SerializeReference] 
        [SerializeField]
        protected List<Bone> _bones = new List<Bone>();
        public List<Bone> Bones => _bones.ToList();

        [SerializeField]
        protected float _defaultValue = 50;
        public float DefaultValue {
            get => _defaultValue;
            set
            {
                if (Equals(_defaultValue, value))
                    return;
                Undo.RecordObject(_controller.RecordTarget, "Set Controller Default");
                _defaultValue = value;
            }
        }

        [SerializeField]
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (Equals(_isEnabled, value))
                    return;

                Undo.RecordObject(_controller.RecordTarget, "Toggle Controller");
                _isEnabled = value;

                // Update Entries
                foreach (var entry in _bones.SelectMany(x => x.Properties.Values).Where(x => x.IsEnabled))
                {
                    entry.Update();
                }
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

                Undo.RecordObject(_controller.RecordTarget, "Expand Controller");
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
                Undo.RecordObject(_controller.RecordTarget, "Slide Controller");
                
                _value = value;

                if (_value == 100)
                    Mode = Mode.Max;
                else if (_value == 0)
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

                Undo.RecordObject(_controller.RecordTarget, "Change Mode");
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
            _controller = controller;
            Name = name;
        }

        public void Update()
        {
            foreach (var entry in _bones.SelectMany(x => x.Properties.Values).Where(x => x.IsEnabled))
            {
                entry.Update();
            }
        }

        public void Add(Bone bone)
        {
            // Check Controller Part within the avatar
            if (!_controller.Avatar.GetComponentsInChildren<Transform>().Contains(bone.Transform))
            {
                throw new Exception("This part doesn't belong to this avatar.");
            }
            // check duplicated part in the controller
            else if (_bones.Select(x => x.Transform).Contains(bone.Transform))
            {
                throw new Exception("Duplicated part.");
            }
            else
            {
                _bones.Add(bone);
            }
        }

        public void Add(Transform part)
        {
            if (!_controller.IsValidBone(part))
                throw new Exception("Component is not valid.");

            Undo.RecordObject(_controller.RecordTarget, "Add Bone");
            Add(new Bone(this, part));
        }

        public void Remove(Bone bone)
        {
            Undo.RecordObject(Controller.RecordTarget, "Remove Bone");
            bone.IsEnabled = false;
            _bones.Remove(bone);
        }

        public void SetDefault()
        {
            DefaultValue = Value;
        }

        public void SetToDefault()
        {
            Undo.RecordObject(_controller.RecordTarget, "Reset Controller");
            Value = DefaultValue;
        }

    }

}