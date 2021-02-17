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
        protected TransformController bone;

        [SerializeField]
        protected Property property;
        public Property Property => property;



        // IsEnabled
        [SerializeField]
        protected bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (Equals(isEnabled, value))
                    return;

                isEnabled = value;
                bone.Handler.Update(property, UpdateHints.ToggledEnable);
            }
        }


        // Min
        [SerializeField]
        protected float min;
        public float Min
        {
            get => min;
            set
            {
                if (Equals(min, value))
                    return;

                min = value;
                bone.Handler.Update(property, UpdateHints.UpdatedChange);
            }
        }

        // Max
        [SerializeField]
        public float max;
        public float Max
        {
            get => max;
            set
            {
                if (Equals(max, value))
                    return;

                max = value;
                bone.Handler.Update(property, UpdateHints.UpdatedChange);
            }
        }

        public float Value => IsOverallEnabled ? GetValue(bone.Controller.Value) : DefaultValue;

        public bool IsOverallEnabled => isEnabled
            && bone.IsEnabled
            && bone.Controller.IsEnabled
            && bone.Controller.AvatarController.IsEnabled;

        public float DefaultValue => property.Type == TransformType.Scale ? 1 : 0;

        public float Change => GetChange(Value, DefaultValue);

        public Entry(TransformController bone, Property property)
        {
            this.bone = bone;
            this.property = property;
            min = DefaultValue;
            max = DefaultValue;
        }

        private float GetChange(float value, float current)
        {
            switch (property.Type)
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

        public float GetValue(float scale)
        {
            switch (property.Type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return Min + (Max - Min) * scale / 100;
                case TransformType.Scale:
                    var logMin = Math.Log(Min, 2);
                    var logMax = Math.Log(Max, 2);
                    return Convert.ToSingle(Math.Pow(2, logMin + (logMax - logMin) * scale / 100));
                default:
                    throw new Exception("Unknown Type.");
            }
            
        }
    }

}