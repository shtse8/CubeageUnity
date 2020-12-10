using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(Controller))]
[CanEditMultipleObjects]
public class ControllerEditor : Editor
{
    Controller controller;

    void OnEnable()
    {
        controller = (Controller) target;
        controller.Avatar = controller.gameObject;
    }

    bool DrawRemoveButton()
    {
        return GUILayout.Button("x", new GUIStyle(GUI.skin.label)
        {
            fixedWidth = EditorGUIUtility.singleLineHeight,
            fixedHeight = EditorGUIUtility.singleLineHeight,
            fontSize = (int)(EditorGUIUtility.singleLineHeight * 0.8),
            alignment = TextAnchor.MiddleCenter,
            hover = new GUIStyleState()
            {
                textColor = Color.red
            },
            margin = new RectOffset(0, 0, 0, 0)
        }) && EditorUtility.DisplayDialog("Controller @ Cubeage", "Are you sure want to remove?", "Yes", "No");
    }

    public override void OnInspectorGUI()
    {
        /*
        if (!EditorGUIUtility.wideMode)
        {
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
        }
        */

        // EditorGUI.BeginChangeCheck();
        // serializedObject.Update();

        using (Style.Horizontal())
        {
            controller.Avatar = (GameObject)EditorGUILayout.ObjectField("Target Avatar", controller.Avatar, typeof(GameObject), true);
        }
        using (Style.Horizontal())
        using (Style.Box())
        {
            EditorGUILayout.LabelField($"Bones: 1");
        }

        Style.Line(1);

        foreach (var currentController in controller.Controllers.ToArray())
        {
            using (Style.Horizontal())
            {
                currentController.isExpanded = EditorGUILayout.Toggle(currentController.isExpanded, new GUIStyle(EditorStyles.foldout), GUILayout.Width(14));
                if (!currentController.isExpanded)
                {
                    currentController.Mode = Mode.View;
                }
                currentController.Name = EditorGUILayout.TextField(currentController.Name);
                using (Style.SetEnable(currentController.Mode == Mode.View))
                {
                    currentController.Value = EditorGUILayout.Slider(currentController.Value, 0, 100);
                    if (currentController.Mode == Mode.View)
                    {
                        currentController.Update();
                    }
                }
                if (DrawRemoveButton())
                {
                    controller.Controllers.Remove(currentController);
                    continue;
                }
            }

            if (currentController.isExpanded)
            {

                // Self implemented indent
                using (Style.Indent())
                using (Style.Box())
                {
                    // anything you do in here will be indented by 20 pixels
                    // relative to stuff outside the top using( xxx) scope

                    currentController.Mode = GUILayout.Toolbar(currentController.Mode.GetValue(), typeof(Mode).GetValues<Mode>().Select(x => x.ToString()).ToArray()).ToEnum<Mode>();

                    switch (currentController.Mode)
                    {
                        case Mode.Min:
                            currentController.Value = 0;
                            break;
                        case Mode.Max:
                            currentController.Value = 100;
                            break;
                    }

                    EditorGUILayout.LabelField($"Bones ({currentController.Bones.Count})", EditorStyles.boldLabel);
                    foreach (var bone in currentController.Bones.ToArray())
                    {
                        using (Style.Horizontal())
                        {
                            bone.isExpanded = EditorGUILayout.Toggle(bone.isExpanded, new GUIStyle(EditorStyles.foldout), GUILayout.Width(14));
                            // GUILayout.Space(-200);
                            using (Style.SetEnable(false))
                            {
                                EditorGUILayout.ObjectField("", bone.Part, typeof(ControllerPart), true);
                            }
                            if (DrawRemoveButton())
                            {
                                currentController.Bones.Remove(bone);
                                continue;
                            }
                        }

                        // get resulting rectangles of items
                        // var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 0);
                        // // items to layout
                        // var items = new List<string>
                        // {
                        //     "One button", "Another button", "Yet another", "Hey there's more", "More!"
                        // };
                        // var style = EditorStyles.miniButton;
                        // var boxes = EditorGUIUtility.GetFlowLayoutedRects(rect, style, 4, 4, items);
                        // rect.height = 1000;
                        // // do actual UI for them
                        // for (var i = 0; i < boxes.Count; ++i)
                        // {
                        //     GUI.Button(boxes[i], items[i], style);
                        // }

                        if (bone.isExpanded)
                        {
                            using (Style.Indent())
                            {
                                using (Style.Horizontal())
                                {
                                    EditorGUILayout.LabelField("Position", GUILayout.MinWidth(50));
                                    using (Style.SetLabelWidth(10))
                                    {
                                        DrawTransformController(bone, Properties.PositionX, currentController.Mode);
                                        DrawTransformController(bone, Properties.PositionY, currentController.Mode);
                                        DrawTransformController(bone, Properties.PositionZ, currentController.Mode);
                                    }
                                }

                                using (Style.Horizontal())
                                {
                                    EditorGUILayout.LabelField("Scale", GUILayout.MinWidth(50));
                                    using (Style.SetLabelWidth(10))
                                    {
                                        DrawTransformController(bone, Properties.ScaleX, currentController.Mode);
                                        DrawTransformController(bone, Properties.ScaleY, currentController.Mode);
                                        DrawTransformController(bone, Properties.ScaleZ, currentController.Mode);
                                    }
                                }
                            }
                        }
                    }

                    using (Style.Indent())
                    {
                        var newControllerPart = (ControllerPart)EditorGUILayout.ObjectField("", null, typeof(ControllerPart), true);
                        if (newControllerPart != null)
                        {
                            // Check Controller Part within the avatar
                            if (!controller.Avatar.GetComponentsInChildren<ControllerPart>().Any(x => x == newControllerPart))
                            {
                                EditorUtility.DisplayDialog("Controller @ Cubeage", "This part doesn't belong to this avatar.", "Okay");
                            }
                            // check duplicated part in the controller
                            else if (currentController.Bones.Any(x => x.Part == newControllerPart))
                            {
                                EditorUtility.DisplayDialog("Controller @ Cubeage", "Duplicated part.", "Okay");
                            }
                            else
                            {
                                currentController.Bones.Add(new Bone(newControllerPart));
                            }
                        }
                    }


                    if (currentController.Mode == Mode.View)
                    {

                        using (Style.Horizontal())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Reset"))
                            {
                                currentController.Value = currentController.DefaultValue;
                            }

                            if (GUILayout.Button("Set Default"))
                            {
                                currentController.DefaultValue = currentController.Value;
                            }
                            GUILayout.FlexibleSpace();
                        }
                    }
                }
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+"))
        {
            controller.Controllers.Add(new BoneController()
            {
                Name = $"Controller {controller.Controllers.Count + 1}"
            });
        }

        GUILayout.EndHorizontal();
        // serializedObject.ApplyModifiedProperties();
    }

    /*
     * Check if the properety is being used by other controllers.
     */
    bool IsPropertyAvailable(Bone bone, Properties property)
    {
        return !controller.Controllers.SelectMany(x => x.Bones)
            .Where(x => !x.Equals(bone))
            .Where(x => x.Part.Equals(bone.Part))
            .Any(x => x.BoneProperties[property].isEnabled);
    }

    /* 
     * Draw Transform Controller with toggle.
     */
    void DrawTransformController(Bone bone, Properties property, Mode mode)
    {
        var boneProperety = bone.BoneProperties[property];
        if (mode == Mode.View)
        {
            using (Style.SetEnable(IsPropertyAvailable(bone, property)))
            {
                boneProperety.isEnabled = EditorGUILayout.Toggle(boneProperety.isEnabled, GUILayout.Width(15));
                if (boneProperety.isEnabled && !IsPropertyAvailable(bone, property))
                {
                    EditorUtility.DisplayDialog("Controller @ Cubeage", "This property is being used by others", "Okay");
                    boneProperety.isEnabled = false;
                }
            }
        }
        using (Style.SetEnable(mode != Mode.View && boneProperety.isEnabled))
        {
            float value = 0;

            // Get Value
            switch (mode)
            {
                case Mode.Min:
                    value = boneProperety.Min;
                    break;
                case Mode.Max:
                    value = boneProperety.Max;
                    break;
                case Mode.View:
                    value = bone.Part.transform.Select(property);
                    break;
            }

            // EditorGUIUtility.labelWidth = 0;
            // EditorGUI.indentLevel = 0;
            var label = "";
            switch (property)
            {
                case Properties.PositionX:
                case Properties.ScaleX:
                    label = "X";
                    break;
                case Properties.PositionY:
                case Properties.ScaleY:
                    label = "Y";
                    break;
                case Properties.PositionZ:
                case Properties.ScaleZ:
                    label = "Z";
                    break;
            }
            value = EditorGUILayout.FloatField(label, value);
            bone.Part.transform.Set(property, value);
            // Update Value
            switch (mode)
            {
                case Mode.Min:
                    boneProperety.Min = value;
                    break;
                case Mode.Max:
                    boneProperety.Max = value;
                    break;
            }
        }
    }
}



public class RectHelper
{
    Rect origin;
    Rect current;
    float spacing;

    public RectHelper(Rect origin, float spacing = 0)
    {
        this.origin = origin;
        this.current = new Rect(origin.x, origin.y, origin.width, 0);
        this.spacing = spacing;
    }

    public void Set(float height)
    {
        this.current.y = this.current.yMax + this.spacing;
        this.current.height = height;
    }

    public Rect Get()
    {
        return this.current;
    }
    public Rect Get(float height)
    {

        this.Set(height);
        return this.current;
    }

    public static explicit operator Rect(RectHelper helper)
    {
        return helper.Get();
    }

}


public static class RechHelperExtensions
{
    public static RectHelper ToHelper(this Rect rect)
    {
        return new RectHelper(rect, EditorGUIUtility.standardVerticalSpacing);
    }

}

public static class EnumExtension
{
    public static string GetDisplayValue<T>(this T value)
    {
        return value.GetType().GetMember(value.ToString())
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()
                   .Name;
    }

    public static int GetValue<T>(this T value) where T : Enum
    {
        return (int)(object) value;
    }

    public static IList<T> GetValues<T>(this Type enumType)
    {
        return Enum.GetValues(enumType).Cast<T>().ToList();
    }
}

public static class Extension
{
    public static T ToEnum<T>(this int value) where T : Enum
    {
        return (T)(object)value;
    }

}


public class DisplayAttribute : Attribute
{
    public DisplayAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public class Disposable : IDisposable
{
    readonly Action Action;

    public Disposable(Action action)
    {
        Action = action;
    }

    public void Dispose()
    {
        Action();
    }

    public static IDisposable Create(Action action)
    {
        return new Disposable(action);
    }
}

public class Style
{
    public static IDisposable SetEnable(bool isEnabled)
    {
        var oldValue = GUI.enabled;
        GUI.enabled = isEnabled;
        return Disposable.Create(() => GUI.enabled = oldValue);
    }

    public static IDisposable SetLabelWidth(float value)
    {
        var oldValue = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = value;
        return Disposable.Create(() => EditorGUIUtility.labelWidth = oldValue);
    }

    public static IDisposable Box()
    {
        return new GUILayout.VerticalScope(new GUIStyle(EditorStyles.helpBox));
    }

    public static IDisposable Indent()
    {
        var cHorizontalScope = new GUILayout.HorizontalScope();
        GUILayout.Space(20f);

        Color[] pix = new Color[] { Color.white };
        Texture2D result = new Texture2D(1, 1);
        result.SetPixels(pix);
        result.Apply();
        var cVerticalScope = new GUILayout.VerticalScope();
        return Disposable.Create(() =>
        {
            cVerticalScope.Dispose();
            cHorizontalScope.Dispose();
        });
    }

    public static IDisposable Horizontal()
    {
        var disposable = new GUILayout.HorizontalScope(new GUIStyle()
        {
            wordWrap = true
        }, new GUILayoutOption[] {
            GUILayout.MinWidth(0),
            GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth)
        });
        return Disposable.Create(() =>
        {
            disposable.Dispose();
            EditorGUILayout.Space();
        });
    }

    public static void Line(float height = 1)
    {
        using (Horizontal())
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.5f));
        }
    }
}