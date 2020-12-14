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
        public PropertiesDictionary Properties = new PropertiesDictionary();
        public bool isExpanded = false;

        public Bone(BoneController boneController, ControllerPart part)
        {
            BoneController = boneController;
            Part = part;
            foreach (var property in EnumHelper.GetValues<Properties>())
            {
                var value = Part.transform.Select(property);
                Properties.Add(property, new Entry(value));
            }
        }

        public void Transform(Properties property, float value)
        {
            Properties[property].Value = value;
            Part.transform.Set(property, value);
        }

        public float Transform(Properties property)
        {
            return Properties[property].Value;
            //return Part.transform.Select(property);
        }

        public bool IsAvailable(Properties property)
        {
            return BoneController.Controller.BoneControllers.SelectMany(x => x.Bones)
                .Where(x => x != this && x.Part == Part)
                .Select(x => x.Properties[property])
                .All(x => !x.IsEnabled);
        }
    }

    [Serializable]
    public class PropertiesDictionary : SerializableDictionary<Properties, Entry> { }

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


    public enum Properties
    {
        PositionX,
        PositionY,
        PositionZ,
        RotationX,
        RotationY,
        RotationZ,
        ScaleX,
        ScaleY,
        ScaleZ
    }

    public static class TransformExtensions
    {
        public static float Select(this Transform transform, Properties property)
        {
            switch (property)
            {
                case Properties.PositionX:
                    return transform.localPosition.x;
                case Properties.PositionY:
                    return transform.localPosition.y;
                case Properties.PositionZ:
                    return transform.localPosition.z;
                case Properties.RotationX:
                    return transform.localEulerAngles.x;
                case Properties.RotationY:
                    return transform.localEulerAngles.y;
                case Properties.RotationZ:
                    return transform.localEulerAngles.z;
                case Properties.ScaleX:
                    return transform.localScale.x;
                case Properties.ScaleY:
                    return transform.localScale.y;
                case Properties.ScaleZ:
                    return transform.localScale.z;
            }
            return 0;
        }

        public static void Set(this Transform transform, Properties property, float value)
        {
            switch (property)
            {
                case Properties.PositionX:
                    transform.localPosition = transform.localPosition.SetX(value);
                    break;
                case Properties.PositionY:
                    transform.localPosition = transform.localPosition.SetY(value);
                    break;
                case Properties.PositionZ:
                    transform.localPosition = transform.localPosition.SetZ(value);
                    break;
                case Properties.RotationX:
                    transform.localEulerAngles = transform.localEulerAngles.SetX(value);
                    break;
                case Properties.RotationY:
                    transform.localEulerAngles = transform.localEulerAngles.SetY(value);
                    break;
                case Properties.RotationZ:
                    transform.localEulerAngles = transform.localEulerAngles.SetZ(value);
                    break;
                case Properties.ScaleX:
                    transform.localScale = transform.localScale.SetX(value);
                    break;
                case Properties.ScaleY:
                    transform.localScale = transform.localScale.SetY(value);
                    break;
                case Properties.ScaleZ:
                    transform.localScale = transform.localScale.SetZ(value);
                    break;
            }
        }

        public static string GetLabel(this Properties property)
        {
            switch (property)
            {
                case Properties.PositionX:
                case Properties.RotationX:
                case Properties.ScaleX:
                    return "X";
                case Properties.PositionY:
                case Properties.RotationY:
                case Properties.ScaleY:
                    return "Y";
                case Properties.PositionZ:
                case Properties.RotationZ:
                case Properties.ScaleZ:
                    return "Z";
                default:
                    throw new Exception($"Unsupported property: {property}");                        
            }
        }

        public static Vector3 SetX(this Vector3 vector, float value)
        {
            vector.x = value;
            return vector;
        }

        public static Vector3 SetY(this Vector3 vector, float value)
        {
            vector.y = value;
            return vector;
        }
        public static Vector3 SetZ(this Vector3 vector, float value)
        {
            vector.z = value;
            return vector;
        }

    }
}