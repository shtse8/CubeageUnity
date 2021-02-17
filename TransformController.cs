using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace Cubeage
{
    [Serializable]
    public class TransformController : IEquatable<TransformController>
    {
        [SerializeReference] 
        [SerializeField]
        protected Controller controller;
        public Controller Controller => controller;

        [SerializeReference] 
        [SerializeField]
        protected TransformHandler transformHandler;
        public TransformHandler Handler => transformHandler;

        [SerializeField]
        [SerializeReference]
        protected List<Entry> properties = new List<Entry>();
        public Dictionary<Property, Entry> Properties => properties.ToDictionary(x => x.Property, x => x);

        // IsEnabled
        [SerializeField]
        protected bool transformChildren;
        public bool TransformChildren
        {
            get => transformChildren;
            set
            {
                if (Equals(transformChildren, value))
                    return;

                transformChildren = value;

                transformHandler.Update(this, UpdateHints.UpdatedTransformChildren);
            }
        }

        [SerializeField]
        public bool isExpanded;
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

        public TransformController(Controller boneController, TransformHandler transformHandler)
        {
            controller = boneController;
            this.transformHandler = transformHandler;
            foreach (var property in Property.GetAll())
            {
                properties.Add(new Entry(this, property));
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

                transformHandler.Update(this, UpdateHints.ToggledEnable);
            }
        }


        public bool IsAvailable(Property property)
        {
            return true;
            // return BoneController.Controller.BoneControllers.SelectMany(x => x.Bones)
            //     .Where(x => x != this && x.Part == Part)
            //     .Select(x => x.Properties[property])
            //     .All(x => !x.IsEnabled);
        }

        public Transform Transform
        {
            get => transformHandler?.Transform;
            set
            {
                var handler = controller.AvatarController.Manager.Get(value);
                if (!Equals(transformHandler, handler))
                {
                    transformHandler?.RemoveTransformController(this);
                    transformHandler = handler;
                    handler.AddTransformController(this);
                }
            }
        }

        public bool Equals(TransformController other)
        {
            return other != null && Equals(controller, other.controller);
        }
    }

}