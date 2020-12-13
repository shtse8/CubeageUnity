using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Cubeage
{
    [CustomEditor(typeof(ControllerPart))]
    [CanEditMultipleObjects]
    public class ControllerPartEditor : Editor
    {
        ControllerPart Part;

        void OnEnable()
        {
            Part = (ControllerPart) target;
        }


        public override void OnInspectorGUI()
        {
            using (Layout.Box())
            {
                EditorGUILayout.LabelField("Done", new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                });
                // foreach(var property in Part.Properties)
                // {
                //     EditorGUILayout.LabelField(property.Key.ToString());
                //     using (Layout.Indent())
                //     {
                //         EditorGUILayout.LabelField($"Max: {property.Value.Max}");
                //         EditorGUILayout.LabelField($"Min: {property.Value.Max}");
                //         EditorGUILayout.LabelField($"Available: {property.Value.IsAvailable()}");
                //         EditorGUILayout.LabelField($"Controller: {property.Value.ControllerId}");
                //         if (!property.Value.IsAvailable())
                //         {
                //             EditorGUILayout.LabelField($"Controller: {property.Value.ControllerId}");
                //         }
                //     }
                // }
            }
        }
    }
}
