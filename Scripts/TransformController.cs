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
        protected Controller _controller;
        public Controller Controller => _controller;

        [SerializeReference] 
        [SerializeField]
        protected TransformHandler _transformHandler;
        public TransformHandler TransformHandler => _transformHandler;

        [SerializeField]
        protected SerializableDictionary<Property, Entry> _properties = new SerializableDictionary<Property, Entry>();
        public Dictionary<Property, Entry> Properties => _properties.ToDictionary(x => x.Key, x => x.Value);

        // IsEnabled
        [SerializeField]
        protected bool _transformChildren = false;
        public bool TransformChildren
        {
            get => _transformChildren;
            set
            {
                if (Equals(_transformChildren, value))
                    return;

                Undo.RecordObject(_controller.AvatarController.RecordTarget, "Toggle Transform Children");
                _transformChildren = value;

                _transformHandler.Update(this);
            }
        }

        [SerializeField]
        public bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (Equals(_isExpanded, value))
                    return;

                Undo.RecordObject(_controller.AvatarController.RecordTarget, "Expand Bone");
                _isExpanded = value;
            }
        }

        public TransformController(Controller boneController, TransformHandler transformHandler)
        {
            _controller = boneController;
            _transformHandler = transformHandler;
            foreach (var property in Property.GetAll())
            {
                _properties.Add(property, new Entry(this, property));
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

                Undo.RecordObject(_controller.AvatarController.RecordTarget, "Toggle Bone");

                _isEnabled = value;

                _transformHandler.Update(this);
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
            get => _transformHandler?.Transform;
            set
            {
                var handler = _controller.AvatarController.Manager.Get(value);
                if (!Equals(_transformHandler, handler))
                {
                    Debug.Log("Update Target");
                    _transformHandler?.RemoveTransformController(this);
                    Debug.Log(_transformHandler?.BoneControllers.Count);
                    Undo.RecordObject(_controller.AvatarController.RecordTarget, "Set Bone");
                    _transformHandler = handler;
                    handler.AddTransformController(this);
                }
            }
        }

        public bool Equals(TransformController other)
        {
            return Equals(_controller, other._controller);
        }
    }

}