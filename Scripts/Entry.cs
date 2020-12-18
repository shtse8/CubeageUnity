using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Cubeage
{
    [Serializable]
    public class Entry
    {
        [SerializeReference]
        protected Bone Bone;

        [SerializeField]
        protected Property Property;

        // IsEnabled
        [SerializeField]
        public bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (Equals(_isEnabled, value))
                    return;

                _isEnabled = value;
            }
        }

        // Min
        [SerializeField]
        protected float _min;
        public float Min
        {
            get => _min;
            set
            {
                if (Equals(_min, value))
                    return;

                _min = value;
                Update();
            }
        }

        // Max
        [SerializeField]
        public float _max;
        public float Max
        {
            get => _max;
            set
            {
                if (Equals(_max, value))
                    return;

                _max = value;
                Update();
            }
        }
        // public float Origin;

        // Value
        protected float _value;
        public float Value
        {
            get => _isEnabled && Bone.IsEnabled && Bone.BoneController.IsEnabled ? GetValue(Bone.BoneController.Value) : DefaultValue;
        }

        public float DefaultValue
        {
            get => Property.Type == TransformType.Scale ? 1 : 0;
        }

        public void Update()
        {
            var change = GetChange(Value);
            TransformCounterBones(Property, change);

            var partValue = GetValue(Bone.Part, change);
            Undo.RecordObject(Bone.Part.transform, "");
            Bone.Part.transform.Set(Property, partValue);

            _value = Value;
        }

        public Entry(Bone bone, Property property, float origin)
        {
            Bone = bone;
            Property = property;
            _min = DefaultValue;
            _max = DefaultValue;
            _value = DefaultValue;
        }

        float GetValue(Component component, float change)
        {
            var value = component.transform.Get(Property);
            switch (Property.Type)
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

        float GetChange(float value)
        {
            switch (Property.Type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return value - _value;
                case TransformType.Scale:
                    return value / _value;
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
                Undo.RecordObject(part.transform, "");
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

        public float GetValue(float scale)
        {
            return Min + (Max - Min) * scale / 100;
        }
    }

}