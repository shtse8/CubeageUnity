using UnityEngine;
using System;


namespace Cubeage
{
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