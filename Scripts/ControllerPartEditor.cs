using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[CustomEditor(typeof(ControllerPart))]
[CanEditMultipleObjects]
public class ControllerPartEditor : Editor
{
    void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.DrawRect(rect, new Color(1f, 1f, 0.8f));
        EditorGUI.LabelField(rect, "Done", new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        });
    }
}
