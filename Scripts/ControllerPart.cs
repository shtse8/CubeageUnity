using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cubeage
{
    public class ControllerPart : MonoBehaviour
    {
    }

    [Serializable]
    public class Bone
    {
        public BoneController BoneController;
        public ControllerPart Part;
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
                    var value = Part.transform.Get(property);
                    Properties.Add(property, new Entry(value));
                }
            }
        }

        public void TransformCounterBones(Property property, float value)
        {
            var valueChanged = value / Properties[property].Value;
            foreach (var part in Part.GetComponentsInChildren<ControllerCounterPart>())
            {
                var oldValue = part.transform.Get(property);
                var newValue = oldValue * 1 / valueChanged;
                part.transform.Set(property, newValue);
            }
        }

        public bool IsValid()
        {
            return Part;
        }

        public void Transform(Property property, float value)
        {
            TransformCounterBones(property, value);
            Properties[property].Value = value;
            Part.transform.Set(property, value);
        }

        public float Transform(Property property)
        {
            return Properties[property].Value;
            //return Part.transform.Select(property);
        }

        public bool IsAvailable(Property property)
        {
            return BoneController.Controller.BoneControllers.SelectMany(x => x.Bones)
                .Where(x => x != this && x.Part == Part)
                .Select(x => x.Properties[property])
                .All(x => !x.IsEnabled);
        }
    }

    [Serializable]
    public class Entry
    {
        public bool IsEnabled = false;
        public float Min;
        public float Max;
        public float Origin;
        public float Value;

        public Entry(float value)
        {
            Min = value;
            Max = value;
            Origin = value;
            Value = value;
        }
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