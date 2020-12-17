using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Cubeage
{
    [Serializable]
    public class Bone
    {
        [SerializeReference] public BoneController BoneController;
        [SerializeReference] public Transform Part;
        public SerializableDictionary<Property, Entry> Properties = new SerializableDictionary<Property, Entry>();
        public bool isExpanded = false;

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