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
        [SerializeReference] public BoneController BoneController;
        [SerializeReference] public Transform Part;
        public SerializableDictionary<Property, Entry> Properties = new SerializableDictionary<Property, Entry>();

        [SerializeField]
        public bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (Equals(_isExpanded, value))
                    return;

                Undo.RecordObject(BoneController.Controller, "Expand Bone");
                _isExpanded = value;
            }
        }

        public Bone(BoneController boneController, Transform part)
        {
            BoneController = boneController;
            Part = part;
            foreach (var type in EnumHelper.GetValues<TransformType>())
            {
                foreach (var direction in EnumHelper.GetValues<Direction>())
                {
                    var property = new Property(type, direction);
                    var origin = Part.transform.Get(property);
                    Properties.Add(property, new Entry(this, property, origin));
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

                Undo.RecordObject(BoneController.Controller, "Toggle Bone");

                _isEnabled = value;

                // Update Entries
                foreach (var entry in Properties.Values.Where(x => x.IsEnabled))
                {
                    entry.Update();
                }
            }
        }


        public bool IsValid()
        {
            return Part;
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