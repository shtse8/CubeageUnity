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
        protected AvatarController avatarController;
        public AvatarController AvatarController => avatarController;

        [SerializeField]
        private  string name;
        public string Name
        {
            get => name;
            set
            {
                if (Equals(name, value))
                    return;

                name = value;
            }
        }
        [SerializeReference] 
        [SerializeField]
        protected List<TransformController> boneControllers = new List<TransformController>();
        public List<TransformController> BoneControllers => boneControllers.ToList();

        [SerializeField]
        protected float defaultValue = 50;
        public float DefaultValue {
            get => defaultValue;
            set
            {
                if (Equals(defaultValue, value))
                    return;

                defaultValue = value;
            }
        }

        [SerializeField]
        private bool isEnabled = true;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (Equals(isEnabled, value))
                    return;

                isEnabled = value;
                avatarController.Manager.Update(boneControllers, UpdateHints.ToggledEnable);
            }
        }


        [SerializeField]
        private float value = 50;

        [SerializeField]
        private Mode mode = Mode.View;

        [SerializeField]
        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (Equals(isExpanded, value))
                    return;

                isExpanded = value;
            }
        }

        public float Value
        {
            get => value;
            set
            {
                if (Equals(this.value, value))
                    return;
                
                this.value = value;

                switch (this.value)
                {
                    case 100:
                        Mode = Mode.Max;
                        break;
                    case 0:
                        Mode = Mode.Min;
                        break;
                    default:
                        Mode = Mode.View;
                        break;
                }
                avatarController.Manager.Update(boneControllers, UpdateHints.UpdatedChange);
            }
        }

        public Mode Mode { 
            get => mode; 
            set
            {
                if (Equals(mode, value))
                    return;

                switch (value)
                {
                    case Mode.Max:
                        Value = 100;
                        break;
                    case Mode.Min:
                        Value = 0;
                        break;
                    case Mode.View:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                mode = value;
            } 
        }

        public Controller(AvatarController avatarController, string name)
        {
            this.avatarController = avatarController;
            this.name = name;
        }

        public void Add(Transform transform)
        {
            var handler = avatarController.Manager.Get(transform);
            boneControllers.Add(handler.CreateTransformController(this));
        }

        public void Remove(TransformController controller)
        {
            controller.IsEnabled = false;

            controller.Handler.RemoveTransformController(controller);
            boneControllers.Remove(controller);
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