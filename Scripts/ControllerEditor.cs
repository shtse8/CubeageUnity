using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace Cubeage
{

    public class MyWindow : EditorWindow
    {
        string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Cubeage")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            MyWindow window = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            myString = EditorGUILayout.TextField("Text Field", myString);

            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle("Toggle", myBool);
            myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            EditorGUILayout.EndToggleGroup();
        }
    }



    [CustomEditor(typeof(Controller))]
    [CanEditMultipleObjects]
    public class ControllerEditor : Editor
    {
        Controller controller;
        // Dictionary<ControllerPart, bool> isExpandedStates = new Dictionary<ControllerPart, bool>();

        void OnEnable()
        {
            controller = (Controller)target;
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

            if (Layout.Button("Window"))
            {
                MyWindow.Init();
            }
            /*
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
            }
            */

            // EditorGUI.BeginChangeCheck();
            // serializedObject.Update();

            using (Layout.Horizontal())
            {
                Layout.Object(controller, x => x.Avatar, "Target Avatar", new ActionRecord(controller, "Set Avatar"));
                // controller.Avatar = (GameObject)EditorGUILayout.ObjectField("Target Avatar", controller.Avatar, typeof(GameObject), true);
            }
            // using (Layout.Horizontal())
            // using (Layout.Box())
            // {
            //     EditorGUILayout.LabelField($"Bones: 1");
            // }

            Layout.Line(1);

            foreach (var currentController in controller.BoneControllers.ToArray())
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(currentController, x => x.isExpanded);
                    if (!currentController.isExpanded)
                    {
                        currentController.Mode = Mode.View;
                    }
                    currentController.Name = EditorGUILayout.TextField(currentController.Name);
                    using (Layout.SetEnable(currentController.Mode == Mode.View))
                    {
                        currentController.Value = EditorGUILayout.Slider(currentController.Value, 0, 100);
                        if (currentController.Mode == Mode.View)
                        {
                            currentController.Update();
                        }
                    }
                    if (DrawRemoveButton())
                    {
                        controller.BoneControllers.Remove(currentController);
                        continue;
                    }
                }

                if (currentController.isExpanded)
                {

                    // Self implemented indent
                    using (Layout.Indent())
                    using (Layout.Box())
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
                            using (Layout.Horizontal())
                            {
                                Layout.Foldout(bone, x => x.isExpanded);
                                // GUILayout.Space(-200);
                                using (Layout.SetEnable(false))
                                {
                                    Layout.Object(bone.Part);
                                }
                                if (DrawRemoveButton())
                                {
                                    currentController.Remove(bone);
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
                                using (Layout.Indent())
                                {
                                    using (Layout.Horizontal())
                                    {
                                        EditorGUILayout.LabelField("Position", GUILayout.MinWidth(50));
                                        using (Layout.SetLabelWidth(10))
                                        {
                                            DrawTransformController(currentController, bone, Properties.PositionX, currentController.Mode);
                                            DrawTransformController(currentController, bone, Properties.PositionY, currentController.Mode);
                                            DrawTransformController(currentController, bone, Properties.PositionZ, currentController.Mode);
                                        }
                                    }

                                    using (Layout.Horizontal())
                                    {
                                        EditorGUILayout.LabelField("Scale", GUILayout.MinWidth(50));
                                        using (Layout.SetLabelWidth(10))
                                        {
                                            DrawTransformController(currentController, bone, Properties.ScaleX, currentController.Mode);
                                            DrawTransformController(currentController, bone, Properties.ScaleY, currentController.Mode);
                                            DrawTransformController(currentController, bone, Properties.ScaleZ, currentController.Mode);
                                        }
                                    }
                                }
                            }
                        }

                        using (Layout.Indent())
                        {
                            var newControllerPart = Layout.Object<ControllerPart>(null);
                            if (newControllerPart != null)
                            {
                                try
                                {
                                    currentController.Add(newControllerPart);
                                } catch (Exception e)
                                {
                                    EditorUtility.DisplayDialog("Controller @ Cubeage", e.Message, "Okay");
                                }
                            }
                        }


                        if (currentController.Mode == Mode.View)
                        {

                            using (Layout.Horizontal())
                            {
                                Layout.FlexibleSpace();
                                if (Layout.Button("Reset"))
                                {
                                    currentController.Value = currentController.DefaultValue;
                                }

                                if (Layout.Button("Set Default"))
                                {
                                    currentController.DefaultValue = currentController.Value;
                                }
                                Layout.FlexibleSpace();
                            }
                        }
                    }
                }
            }

            using (Layout.Horizontal())
            {
                Layout.FlexibleSpace();
                if (Layout.Button("+"))
                {
                    controller.AddController();
                }
            }
        }

        /* 
         * Draw Transform Controller with toggle.
         */
        void DrawTransformController(BoneController boneController, Bone bone, Properties property, Mode mode)
        {
            var entry = bone.Properties[property];
            if (mode == Mode.View)
            {
                using (Layout.SetEnable(entry.IsEnabled || bone.IsAvailable(property)))
                {
                    Layout.Toggle(entry, x => x.IsEnabled, new ActionRecord(target, "Toggle Property"));
                }
            }

            using (Layout.SetEnable(mode != Mode.View && entry.IsEnabled))
            {
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
                switch (mode)
                {
                    case Mode.Min:
                        Layout.Float(entry, x => x.Min, label, new ActionRecord(target, "Change Transform"));
                        bone.Transform(property, entry.Min);
                        break;
                    case Mode.Max:
                        Layout.Float(entry, x => x.Max, label, new ActionRecord(target, "Change Transform"));
                        bone.Transform(property, entry.Max);
                        break;
                    case Mode.View:
                        Layout.Float(bone.Transform(property), label);
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

    public class Layout
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

            // Color[] pix = new Color[] { Color.white };
            // Texture2D result = new Texture2D(1, 1);
            // result.SetPixels(pix);
            // result.Apply();
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

        public static void Toggle<T>(T target, Expression<Func<T, bool>> expression, ActionRecord record = null)
        {
            ValueChanged(target, expression, x => EditorGUILayout.Toggle(x, GUILayout.Width(15)), record);
        }

        public static void Foldout<T>(T target, Expression<Func<T, bool>> expression, ActionRecord record = null)
        {
            ValueChanged(target, expression, x => EditorGUILayout.Toggle(x, new GUIStyle(EditorStyles.foldout), GUILayout.Width(14)), record);
        }


        public static void Float<T>(T target, Expression<Func<T, float>> expression, string label = null, ActionRecord record = null)
        {
            ValueChanged(target, expression, x => Float(x, label), record);
        }

        public static float Float(float value, string label = null)
        {
            return EditorGUILayout.FloatField(label, value);
        }

        public static void Object<TTarget, T>(TTarget target, Expression<Func<TTarget, T>> expression, string label = null, ActionRecord record = null) where T : UnityEngine.Object
        {
            ValueChanged(target, expression, x => Object(x, label), record);
        }

        public static T Object<T>(T value, string label = null) where T : UnityEngine.Object
        {
            return (T)EditorGUILayout.ObjectField(label, value, typeof(T), true);
        }

        public static bool Button(string label)
        {
            return GUILayout.Button(label);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }
            
        private static void ValueChanged<TTarget, T>(TTarget target, Expression<Func<TTarget, T>> expression, Func<T, T> layoutGenerator, ActionRecord record = null)
        {
            var selector = expression.Compile();
            var oldValue = selector(target);
            var newValue = layoutGenerator(oldValue);

            // Value changed.
            if (!newValue.Equals(oldValue))
            {
                Debug.Log(newValue);
                // Make Undo Record
                if (record != null)
                    Undo.RecordObject(record.Target, record.ActionName);
                target.SetValue(expression, newValue);
            }
        }
    }

    public class Reference<T>
    {
        public T Value { get; set; }
        public Reference(T value)
        {
            Value = value;
        }

    }

    public static class Reference
    {
        public static Reference<T> Wrap<T>(T value)
        {
            return new Reference<T>(value);
        }
    }

    public class ActionRecord
    {
        public UnityEngine.Object Target { get; }
        public string ActionName { get; }

        public ActionRecord(UnityEngine.Object target, string actionName)
        {
            Target = target;
            ActionName = actionName;
        }
    }
}
