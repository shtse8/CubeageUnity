using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;

public class Controller : MonoBehaviour
{
    public GameObject Avatar;
    public List<BoneController> Controllers = new List<BoneController>();


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

}

[Serializable]
public class BoneController
{
    public string Name = "";
    public List<Bone> Bones = new List<Bone>();
    public bool isExpanded = false;
    public float Value = 50;
    public float DefaultValue = 50;
    public Mode Mode = Mode.View;

    public void Update()
    {
        var ratio = Value / 100;
        foreach (var bone in Bones)
        {
            foreach (var dict in bone.BoneProperties)
            {
                var property = dict.Key;
                var boneProperty = dict.Value;
                if (!boneProperty.isEnabled)
                    continue;

                float value = boneProperty.Min + (boneProperty.Max - boneProperty.Min) * ratio;
                bone.Part.transform.Set(property, value);
            }
        }
    }
}

[Serializable]
public class BonePropertiesDictionary : SerializableDictionary<Properties, BoneProperty> { }

[Serializable]
public class Bone
{
    public ControllerPart Part;
    public BonePropertiesDictionary BoneProperties = new BonePropertiesDictionary();
    public bool isExpanded = false;

    public Bone(ControllerPart part)
    {
        Part = part;
        foreach(var property in typeof(Properties).GetValues<Properties>().ToList())
        {
            var value = part.transform.Select(property);
            var boneProperty = new BoneProperty
            {
                Min = value,
                Max = value
            };
            BoneProperties.Add(property, boneProperty);
        }
    }
}

[Serializable]
public class BoneProperty
{
    public bool isEnabled = false;
    public float Min;
    public float Max;
}


public enum Properties
{
    PositionX,
    PositionY,
    PositionZ,
    ScaleX,
    ScaleY,
    ScaleZ
}

public enum Mode
{
    View,
    Min,
    Max
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