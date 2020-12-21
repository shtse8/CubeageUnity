using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace Cubeage
{
    [Serializable]
    public class Entry
    {
        [SerializeReference]
        [SerializeField]
        protected Bone _bone;

        [SerializeField]
        protected Property _property;




        // IsEnabled
        [SerializeField]
        protected bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (Equals(_isEnabled, value))
                    return;

                Undo.RecordObject(_bone.Controller.AvatarController.RecordTarget, "Toggle Property");
                _isEnabled = value;
                Update();
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

                Undo.RecordObject(_bone.Controller.AvatarController.RecordTarget, "Change Min");
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

                Undo.RecordObject(_bone.Controller.AvatarController.RecordTarget, "Change Max");
                _max = value;
                Update();
            }
        }
        // public float Origin;

        // Value
        [SerializeField]
        protected float _value;
        public float Value => IsOverallEnabled ? GetValue(_bone.Controller.Value) : DefaultValue;

        public bool IsOverallEnabled => _isEnabled
            && _bone.IsEnabled
            && _bone.Controller.IsEnabled
            && _bone.Controller.AvatarController.IsEnabled;

        public float DefaultValue => _property.Type == TransformType.Scale ? 1 : 0;

        public static void Update(AvatarController avatarController, Transform transform, Property property)
        {
            var value = avatarController.ValidBones[transform].Get(property);
            foreach (var entry in avatarController.Controllers.SelectMany(x => x.Bones)
                    .Where(x => Equals(x.Transform, transform))
                    .Select(x => x.Properties[property])
                    .Where(x => x.IsEnabled))
            {
                value = GetValue(property.Type, value, entry.Change);
            }

            // Find Parent Controller
            var parent = transform.parent;
            while (parent && !avatarController.ValidBones.ContainsKey(parent))
            {
                parent = parent.parent;
            }
            if (parent)
            {
                foreach (var entry in avatarController.Controllers.SelectMany(x => x.Bones)
                    .Where(x => Equals(x.Transform, parent))
                    .Where(x => !x.TransformChildren)
                    .Select(x => x.Properties[property])
                    .Where(x => x.IsEnabled))
                {
                    value = GetValue(property.Type, value, GetCounterChange(property.Type, entry.Change));
                }
            }

            transform.Set(property, value);

            // Update Children
            foreach (var part in SearchBonesRecursive(avatarController, transform))
            {
                Update(avatarController, part, property);
            }

        }


        // TODO: Optimize the update from origin and transform children
        public void Update()
        {
            Update(_bone.Controller.AvatarController, _bone.Transform, _property);
            // var origin = _bone.Controller.AvatarController.ValidBones[_bone.Transform].Get(_property);
            // var value = GetValue(origin, Change);
            // // Find Parent Controller
            // var parent = _bone.Transform.parent;
            // while (parent && _bone.Controller.AvatarController.ValidBones.ContainsKey(parent))
            // {
            //     parent = parent.parent;
            // }
            // if (parent)
            // {
            //     foreach(var entry in _bone.Controller.AvatarController.Controllers.SelectMany(x => x.Bones)
            //         .Where(x => Equals(x.Transform, parent))
            //         .Select(x => x.Properties[_property])
            //         .Where(x => x.IsEnabled)) 
            //     {
            //         value = GetValue(value, GetCounterChange(_property, entry.Change));
            //     }
            // }
            // _bone.Transform.transform.Set(_property, value);
            // 
            // _value = Value;
            // 
            // foreach (var part in SearchBonesRecursive(_bone.Transform))
            // {
            //     foreach (var entry in _bone.Controller.AvatarController.Controllers.SelectMany(x => x.Bones)
            //             .Where(x => Equals(x.Transform, part))
            //             .Select(x => x.Properties[_property])
            //             .Where(x => x.IsEnabled))
            //     {
            // 
            //         entry.Update();
            //     }
            // }

            /*
            var change = GetChange(Value, _value);

            // if (_bone.TransformChildren)
            TransformCounterBones(_property, change);

            var partValue = GetValue(_bone.Transform, change);
            Undo.RecordObject(_bone.Transform.transform, "");
            _bone.Transform.transform.Set(_property, partValue);

            _value = Value;
            */
        }

        public Entry(Bone bone, Property property)
        {
            _bone = bone;
            _property = property;
            _min = DefaultValue;
            _max = DefaultValue;
            _value = DefaultValue;
        }

        static float GetValue(TransformType type, float value, float change)
        {
            switch (type)
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

        float GetValue(float value, float change)
        {
            switch (_property.Type)
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

        float GetValue(Component component, float change)
        {
            var value = component.transform.Get(_property);
            return GetValue(value, change);
        }

        public float Change => GetChange(Value, DefaultValue);

        float GetChange(float value, float current)
        {
            switch (_property.Type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return value - current;
                case TransformType.Scale:
                    return value / current;
                default:
                    throw new Exception("Unknown Type.");
            }
        }


        static IEnumerable<Transform> SearchBonesRecursive(AvatarController avatarController, Transform transform)
        {
            var bones = new List<Transform>();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (avatarController.ValidBones.ContainsKey(child))
                    bones.Add(child);
                else
                    bones.AddRange(SearchBonesRecursive(avatarController, child));
            }
            return bones;
        }

        IEnumerable<Transform> SearchBonesRecursive(Transform transform)
        {
            var bones = new List<Transform>();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (_bone.Controller.AvatarController.ValidBones.ContainsKey(child))
                    bones.Add(child);
                else
                    bones.AddRange(SearchBonesRecursive(child));
            }
            return bones;
        }

        void TransformCounterBones(Property property, float change)
        {
            change = GetCounterChange(property.Type, change);
            foreach (var part in SearchBonesRecursive(_bone.Transform))
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

        static float GetCounterChange(TransformType type, float change)
        {
            switch (type)
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