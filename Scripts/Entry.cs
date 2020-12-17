using UnityEngine;
using System;
using System.Collections.Generic;

namespace Cubeage
{
    [Serializable]
    public class Entry
    {
        [SerializeReference]
        protected Bone Bone;

        protected Property Property;

        [SerializeField]
        public bool _isEnabled = false;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (value)
                {
                    Value = GetValue(Bone.BoneController.Value);
                }
                else
                {
                    Value = DefaultValue;
                }
            }
        }

        [SerializeField]
        protected float _min;
        public float Min
        {
            get => _min;
            set
            {
                _min = value;
                Value = value;
            }
        }


        [SerializeField]
        public float _max;
        public float Max
        {
            get => _max;
            set
            {
                _max = value;
                Value = value;
            }
        }
        public float Origin;

        [SerializeField]
        public float _value;

        public float Value
        {
            get => _value;
            set
            {

                var change = GetChange(value);
                TransformCounterBones(Property, change);

                var partValue = GetValue(Bone.Part, change);
                Bone.Part.transform.Set(Property, partValue);

                _value = value;
            }
        }

        float GetValue(Component component, float change)
        {
            var value = component.transform.Get(Property);
            switch (Property.Type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return Value + change;
                case TransformType.Scale:
                    return Value * change;
                default:
                    throw new Exception("Unknown Type.");
            }
        }

        public Entry(Bone bone, Property property, float origin)
        {
            Bone = bone;
            Property = property;
            _min = DefaultValue;
            _max = DefaultValue;
            _value = DefaultValue;
            Origin = origin;
        }

        float DefaultValue
        {
            get => Property.Type == TransformType.Scale ? 1 : 0;
        }

        float GetChange(float value)
        {
            switch (Property.Type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return value - Value;
                case TransformType.Scale:
                    return value / Value;
                default:
                    throw new Exception("Unknown Type.");
            }
        }


        IEnumerable<Transform> SearchBonesRecursive(Transform transform)
        {
            var bones = new List<Transform>();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (Bone.BoneController.Controller.ValidBones.Contains(child))
                    bones.Add(child);
                else
                    bones.AddRange(SearchBonesRecursive(child));
            }
            return bones;
        }

        void TransformCounterBones(Property property, float change)
        {
            change = GetCounterChange(property, change);
            foreach (var part in SearchBonesRecursive(Bone.Part))
            {
                var newValue = GetValue(part, property, change);
                part.transform.Set(property, newValue);
            }
        }

        float GetValue(Transform component, Property property, float change)
        {
            var value = component.transform.Get(property);
            switch (property.Type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return value + change;
                case TransformType.Scale:
                    return value * change;
                default:
                    throw new Exception("Unknown Type.");
            }
        }

        float GetCounterChange(Property property, float change)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return -change;
                case TransformType.Scale:
                    return 1 / change;
                default:
                    throw new Exception("Unknown Type.");
            }
        }

        float GetValue(float scale)
        {
            return Min + (Max - Min) * scale / 100;
        }
    }

}