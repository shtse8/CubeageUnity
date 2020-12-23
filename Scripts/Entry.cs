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
        protected TransformController _bone;

        [SerializeField]
        protected Property _property;
        public Property Property => _property;



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
                _bone.TransformHandler.Update(_property, UpdateHints.ToggledEnable);
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
                _bone.TransformHandler.Update(_property, UpdateHints.UpdatedChange);
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
                _bone.TransformHandler.Update(_property, UpdateHints.UpdatedChange);
            }
        }

        public float Value => IsOverallEnabled ? GetValue(_bone.Controller.Value) : DefaultValue;

        public bool IsOverallEnabled => _isEnabled
            && _bone.IsEnabled
            && _bone.Controller.IsEnabled
            && _bone.Controller.AvatarController.IsEnabled;

        public float DefaultValue => _property.Type == TransformType.Scale ? 1 : 0;

        public float Change => GetChange(Value, DefaultValue);

        public Entry(TransformController bone, Property property)
        {
            _bone = bone;
            _property = property;
            _min = DefaultValue;
            _max = DefaultValue;
        }

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

        public float GetValue(float scale)
        {
            switch (_property.Type)
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