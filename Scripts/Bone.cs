using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace Cubeage
{
    [Serializable]
    public class Bone
    {
        [SerializeReference] 
        [SerializeField]
        protected Controller _controller;
        public Controller Controller => _controller;

        [SerializeReference] 
        [SerializeField]
        protected Transform _transform;
        public Transform Transform => _transform;

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

                // Update Entries
                foreach (var entry in _properties.Values.Where(x => x.IsEnabled))
                {
                    entry.Update();
                }
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

        public Bone(Controller boneController, Transform part)
        {
            _controller = boneController;
            _transform = part;
            foreach (var type in EnumHelper.GetValues<TransformType>())
            {
                foreach (var direction in EnumHelper.GetValues<Dimension>())
                {
                    var property = new Property(type, direction);
                    _properties.Add(property, new Entry(this, property));
                }
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

                // Update Entries
                foreach (var entry in _properties.Values.Where(x => x.IsEnabled))
                {
                    entry.Update();
                }
            }
        }


        public bool IsValid()
        {
            return _transform;
        }

        public bool IsAvailable(Property property)
        {
            return true;
            // return BoneController.Controller.BoneControllers.SelectMany(x => x.Bones)
            //     .Where(x => x != this && x.Part == Part)
            //     .Select(x => x.Properties[property])
            //     .All(x => !x.IsEnabled);
        }


    }

}