using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;


namespace Cubeage
{
    public class Controller : MonoBehaviour
    {
        [SerializeReference] public GameObject Avatar;
        [SerializeReference] public List<BoneController> BoneControllers = new List<BoneController>();

        void Reset()
        {
            Avatar = gameObject;

        }

        public void AddController()
        {
            BoneControllers.Add(new BoneController(this, $"Controller {BoneControllers.Count + 1}"));
        }

    }



    [Serializable]
    public class BoneController : ISerializationCallbackReceiver
    {
        [SerializeReference] public Controller Controller;
        public string Name = "";
        [SerializeReference] public List<Bone> Bones = new List<Bone>();
        
        public float DefaultValue = 50;

        private bool _isExpanded = false;
        private float _value = 50;
        private Mode _mode = Mode.View;

        public bool IsExpanded
        {
            get => _isExpanded;
            set {
                _isExpanded = value;
                if (!value)
                    Mode = Mode.View;
            }
        }
        public float Value
        {
            get => _value;
            set
            {
                _value = value;
                // if (Mode == Mode.View)
                Update();
            }
        }

        public Mode Mode { 
            get => _mode; 
            set {
                Debug.Log($"Setting: {value}");
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

        public BoneController(Controller controller, string name)
        {
            Controller = controller;
            Name = name;
        }

        void RemoveInvalidBones()
        {
            foreach (var x in Bones.Where(x => !x.IsValid()).ToArray())
            {
                Remove(x);
            }
        }


        public IEnumerable<Bone> GetValidBones()
        {
            RemoveInvalidBones();
            return Bones.ToArray();
        }

        public void Update()
        {
            var ratio = Value / 100;
            foreach (var bone in Bones)
            {
                foreach ((var property, var boneProperty) in bone.Properties.Where(x => x.Value.IsEnabled))
                {
                    float value = boneProperty.Min + (boneProperty.Max - boneProperty.Min) * ratio;
                    boneProperty.Value = value;
                }
            }
        }


        public void Add(Bone bone)
        {
            // Check Controller Part within the avatar
            if (!Controller.Avatar.GetComponentsInChildren<ControllerPart>().Contains(bone.Part))
            {
                throw new Exception("This part doesn't belong to this avatar.");
            }
            // check duplicated part in the controller
            else if (Bones.Select(x => x.Part).Contains(bone.Part))
            {
                throw new Exception("Duplicated part.");
            }
            else
            {
                Bones.Add(bone);
            }
        }

        public void SetDefault()
        {
            DefaultValue = Value;
        }

        public void SetValue(float value, bool noUpdate = false)
        {
            Value = value;
            if (!noUpdate)
                Update();
        }

        public void Reset()
        {
            SetValue(DefaultValue);
        }

        public void Add(ControllerPart part)
        {
            Add(new Bone(this, part));
        }

        public void Remove(Bone bone)
        {
            Bones.Remove(bone);
        }

        public void OnBeforeSerialize()
        {
        }


        public void OnAfterDeserialize()
        {
            Update();
        }
    }

    public enum Mode
    {
        View,
        Min,
        Max
    }



    [Serializable]
    public class Bone
    {
        [SerializeReference] public BoneController BoneController;
        [SerializeReference] public ControllerPart Part;
        public SerializableDictionary<Property, Entry> Properties = new SerializableDictionary<Property, Entry>();
        public bool isExpanded = false;

        public Bone(BoneController boneController, ControllerPart part)
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

        public void TransformCounterBones(Property property, float change)
        {
            change = GetCounterChange(property, change);
            foreach (var part in Part.GetComponentsInChildren<ControllerPart>()
                                     .Where(x => x != Part)
                                     .Where(x => x.GetComponentsInParent<ControllerPart>().Where(y => y != x).First().Equals(Part)))
            {
                var newValue = GetValue(part, property, change);
                part.transform.Set(property, newValue);
            }
        }

        float GetValue(Component component, Property property, float change)
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

        public bool IsValid()
        {
            return Part;
        }

        public bool IsAvailable(Property property)
        {
            return BoneController.Controller.BoneControllers.SelectMany(x => x.Bones)
                .Where(x => x != this && x.Part == Part)
                .Select(x => x.Properties[property])
                .All(x => !x.IsEnabled);
        }

        public float GetValue(Property property, float scale)
        {
            var targetProperty = Properties[property];
            return targetProperty.Min + (targetProperty.Max - targetProperty.Min) * scale / 100;
        }

    }

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
                    Value = Bone.GetValue(Property, Bone.BoneController.Value);
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
                Bone.TransformCounterBones(Property, change);

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


    }

    public class Range
    {
        public float? Min;
        public float? Max;
    }

    [Serializable]
    public struct Property
    {
        public TransformType Type;
        public Direction Direction;

        public Property(TransformType type, Direction direction)
        {
            Type = type;
            Direction = direction;
        }

        public static explicit operator Property(Tuple<TransformType, Direction> tuple)
        {
            return new Property(tuple.Item1, tuple.Item2);
        }
    }

    public enum TransformType
    {
        Position,
        Rotation,
        Scale
    }

    public enum Direction
    {
        X,
        Y,
        Z
    }

    public static class TransformExtensions
    {
        public static float Get(this Transform transform, Property property)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                    return transform.localPosition.Get(property.Direction);
                case TransformType.Rotation:
                    return transform.localEulerAngles.Get(property.Direction);
                case TransformType.Scale:
                    return transform.localScale.Get(property.Direction);
            }
            return 0;
        }

        public static void Set(this Transform transform, Property property, float value)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                    transform.localPosition = transform.localPosition.Set(property.Direction, value);
                    break;
                case TransformType.Rotation:
                    transform.localEulerAngles = transform.localEulerAngles.Set(property.Direction, value);
                    break;
                case TransformType.Scale:
                    transform.localScale = transform.localScale.Set(property.Direction, value);
                    break;
            }
        }

        public static float Get(this Vector3 vector, Direction direction)
        {
            switch (direction)
            {
                case Direction.X:
                    return vector.x;
                case Direction.Y:
                    return vector.y;
                case Direction.Z:
                    return vector.z;
                default:
                    throw new Exception($"Unknown Direction: {direction}");
            }
        }

        public static Vector3 Set(this Vector3 vector, Direction direction, float value)
        {
            switch (direction)
            {
                case Direction.X:
                    vector.x = value;
                    break;
                case Direction.Y:
                    vector.y = value;
                    break;
                case Direction.Z:
                    vector.z = value;
                    break;
                default:
                    throw new Exception($"Unknown Direction: {direction}");
            }
            return vector;
        }

    }

}