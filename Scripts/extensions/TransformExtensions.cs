﻿using UnityEngine;
using System;


namespace Cubeage
{
    public static class TransformExtensions
    {
        public static float Get(this TransformData transform, Property property)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                    return transform.localPosition.Get(property.Dimension);
                case TransformType.Rotation:
                    return transform.localEulerAngles.Get(property.Dimension);
                case TransformType.Scale:
                    return transform.localScale.Get(property.Dimension);
            }
            return 0;
        }

        public static void Set(this TransformData transform, Property property, float value)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                    transform.localPosition = transform.localPosition.Set(property.Dimension, value);
                    break;
                case TransformType.Rotation:
                    transform.localEulerAngles = transform.localEulerAngles.Set(property.Dimension, value);
                    break;
                case TransformType.Scale:
                    transform.localScale = transform.localScale.Set(property.Dimension, value);
                    break;
            }
        }

        public static float Get(this Transform transform, Property property)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                    return transform.localPosition.Get(property.Dimension);
                case TransformType.Rotation:
                    return transform.localEulerAngles.Get(property.Dimension);
                case TransformType.Scale:
                    return transform.localScale.Get(property.Dimension);
            }
            return 0;
        }

        public static void Set(this Transform transform, Property property, float value)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                    transform.localPosition = transform.localPosition.Set(property.Dimension, value);
                    break;
                case TransformType.Rotation:
                    transform.localEulerAngles = transform.localEulerAngles.Set(property.Dimension, value);
                    break;
                case TransformType.Scale:
                    transform.localScale = transform.localScale.Set(property.Dimension, value);
                    break;
            }
        }

        public static float Get(this Vector3 vector, Dimension direction)
        {
            switch (direction)
            {
                case Dimension.X:
                    return vector.x;
                case Dimension.Y:
                    return vector.y;
                case Dimension.Z:
                    return vector.z;
                default:
                    throw new Exception($"Unknown Direction: {direction}");
            }
        }

        public static Vector3 Set(this Vector3 vector, Dimension direction, float value)
        {
            switch (direction)
            {
                case Dimension.X:
                    vector.x = value;
                    break;
                case Dimension.Y:
                    vector.y = value;
                    break;
                case Dimension.Z:
                    vector.z = value;
                    break;
                default:
                    throw new Exception($"Unknown Direction: {direction}");
            }
            return vector;
        }

    }

}