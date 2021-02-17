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

                _name = value;
            }
        }
        [SerializeReference] 
        [SerializeField]
        protected List<TransformController> _boneControllers = new List<TransformController>();
        public List<TransformController> BoneControllers => _boneControllers.ToList();

        [SerializeField]
        protected float _defaultValue = 50;
        public float DefaultValue {
            get => _defaultValue;
            set
            {
                if (Equals(_defaultValue, value))
                    return;

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

                _isEnabled = value;
                _avatarController.Manager.Update(_boneControllers, UpdateHints.ToggledEnable);
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

                if (_value == 100)
                    Mode = Mode.Max;
                else if (_value == 0)
                    Mode = Mode.Min;
                else
                    Mode = Mode.View;

                _avatarController.Manager.Update(_boneControllers, UpdateHints.UpdatedChange);
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

        public Controller(AvatarController avatarController, string name)
        {
            _avatarController = avatarController;
            _name = name;
        }

        public void Add(Transform transform)
        {
            var handler = _avatarController.Manager.Get(transform);
            _boneControllers.Add(handler.CreateTransformController(this));
        }

        public void Remove(TransformController controller)
        {
            controller.IsEnabled = false;

            controller.Handler.RemoveTransformController(controller);
            _boneControllers.Remove(controller);
        }

        public void SetDefault()
        {
            DefaultValue = Value;
        }

        public void SetToDefault()
        {
            Value = DefaultValue;
        }

    }

}