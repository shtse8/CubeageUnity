using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace Cubeage
{
    [Serializable]
    public class Controller
    {
        [SerializeField]
        [SerializeReference]
        protected AvatarController _avatarController;
        public AvatarController AvatarController => _avatarController;

        [SerializeField]
        private  string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (Equals(_name, value))
                    return;
                Undo.RecordObject(_avatarController.RecordTarget, "Change Controller Name");
                _name = value;
            }
        }
        [SerializeReference] 
        [SerializeField]
        protected List<TransformController> _boneControllers = new List<TransformController>();
        public List<TransformController> Bones => _boneControllers.ToList();

        [SerializeField]
        protected float _defaultValue = 50;
        public float DefaultValue {
            get => _defaultValue;
            set
            {
                if (Equals(_defaultValue, value))
                    return;
                Undo.RecordObject(_avatarController.RecordTarget, "Set Controller Default");
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

                Undo.RecordObject(_avatarController.RecordTarget, "Toggle Controller");
                _isEnabled = value;
                Update();
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

                Undo.RecordObject(_avatarController.RecordTarget, "Expand Controller");
                _isExpanded = value;
            }
        }

        public float Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;
                Undo.RecordObject(_avatarController.RecordTarget, "Slide Controller");
                
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

                Undo.RecordObject(_avatarController.RecordTarget, "Change Mode");
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

        public Controller(AvatarController avatarController, string name)
        {
            _avatarController = avatarController;
            _name = name;
        }

        public void Update()
        {
            _avatarController.Manager.Update(_boneControllers);
        }

        public void Add(Transform transform)
        {
            var handler = _avatarController.Manager.Get(transform);
            Undo.RecordObject(_avatarController.RecordTarget, "Add Bone");
            _boneControllers.Add(handler.CreateTransformController(this));
        }

        public void Remove(TransformController controller)
        {
            Undo.RecordObject(_avatarController.RecordTarget, "Remove Bone");
            controller.IsEnabled = false;

            controller.TransformHandler.RemoveTransformController(controller);
            _boneControllers.Remove(controller);
        }

        public void SetDefault()
        {
            DefaultValue = Value;
        }

        public void SetToDefault()
        {
            Undo.RecordObject(_avatarController.RecordTarget, "Reset Controller");
            Value = DefaultValue;
        }

    }

}