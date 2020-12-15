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
            }) && Confirm("Are you sure want to remove?");
        }

        public override void OnInspectorGUI()
        {

            // if (Layout.Button("Window"))
            // {
            //     MyWindow.Init();
            // }
            using (Layout.Horizontal())
            {
                Layout.Object(controller, x => x.Avatar, "Target Avatar", UndoRecord("Set Avatar"));
            }

            using (Layout.Toolbar())
            {
                if (Layout.ToolbarButton("Add"))
                {
                    AddUndo("Add Controller");
                    controller.AddController();
                }
                if (Layout.ToolbarButton("Reset"))
                {
                }
                Layout.FlexibleSpace();
            }
            
            foreach (var currentController in controller.BoneControllers.ToArray())
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(currentController, x => x.isExpanded, UndoRecord("Toggle Controller")).OnChanged(x =>
                    {
                        if (!x) currentController.Mode = Mode.View;
                    });
                    Layout.Text(currentController, x => x.Name);
                    using (Layout.SetEnable(currentController.Mode == Mode.View))
                    {
                        Layout.Slider(currentController, x => x.Value, 0, 100, UndoRecord("Slide Controller")).OnChanged(_ =>
                        {
                            if (currentController.Mode == Mode.View)
                                currentController.Update();
                        });
                    }
                    if (DrawRemoveButton())
                    {
                        AddUndo("Remove Bone");
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
                        Layout.EnumToolbar(currentController, x => x.Mode, UndoRecord("Change Mode"));

                        Layout.Label($"Bones ({currentController.Bones.Count})", EditorStyles.boldLabel);
                        foreach (var bone in currentController.GetValidBones())
                        {
                            using (Layout.Horizontal())
                            {
                                Layout.Foldout(bone, x => x.isExpanded, UndoRecord("Expand Bone"));
                                using (Layout.SetEnable(false))
                                {
                                    Layout.Object(bone.Part);
                                }
                                if (DrawRemoveButton())
                                {
                                    AddUndo("Remove Bone");
                                    currentController.Remove(bone);
                                    continue;
                                }
                            }

                            if (bone.isExpanded)
                            {
                                using (Layout.Indent())
                                {
                                    foreach (var type in EnumHelper.GetValues<TransformType>())
                                    {
                                        using (Layout.Horizontal())
                                        {
                                            Layout.Label(type.ToString());
                                            using (Layout.SetLabelWidth(10))
                                            {
                                                foreach (var direction in EnumHelper.GetValues<Direction>())
                                                {
                                                    DrawTransformController(bone, new Property(type, direction), currentController.Mode);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        using (Layout.Indent())
                        using (Layout.Horizontal())
                        {
                            var newControllerPart = Layout.Object<ControllerPart>();
                            if (newControllerPart != null)
                            {
                                try
                                {
                                    AddUndo("Add Bone");
                                    currentController.Add(newControllerPart);
                                } catch (Exception e)
                                {
                                    Alert(e.Message);
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
                                    AddUndo("Reset Controller");
                                    currentController.Reset();
                                }

                                if (Layout.Button("Set Default"))
                                {
                                    AddUndo("Set Controller Default");
                                    currentController.SetDefault();
                                }
                                Layout.FlexibleSpace();
                            }
                        }
                    }
                }
            }

        }

        ActionRecord UndoRecord(string name)
        {
            return new ActionRecord(controller, name);
        }

        void AddUndo(string name)
        {
            Undo.RecordObject(controller, name);
        }

        bool Confirm(string message)
        {
            return EditorUtility.DisplayDialog("Controller @ Cubeage", message, "Yes", "No");
        }

        void Alert(string message)
        {
            EditorUtility.DisplayDialog("Controller @ Cubeage", message, "Okay");
        }

        /* 
         * Draw Transform Controller with toggle.
         */
        void DrawTransformController(Bone bone, Property property, Mode mode)
        {
            var entry = bone.Properties[property];
            if (mode == Mode.View)
            {
                using (Layout.SetEnable(entry.IsEnabled || bone.IsAvailable(property)))
                {
                    Layout.Toggle(entry, x => x.IsEnabled, UndoRecord("Toggle Property"));
                }
            }

            using (Layout.SetEnable(mode != Mode.View && entry.IsEnabled))
            {
                switch (mode)
                {
                    case Mode.Min:
                        Layout.Float(entry, x => x.Min, property.Direction.ToString(), UndoRecord("Change Transform"))
                              .OnChanged(x => bone.Transform(property, x));
                        break;
                    case Mode.Max:
                        Layout.Float(entry, x => x.Max, property.Direction.ToString(), UndoRecord("Change Transform"))
                              .OnChanged(x => bone.Transform(property, x));
                        break;
                    case Mode.View:
                        Layout.Float(bone.Transform(property), property.Direction.ToString());
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

    public class LayoutCallback<T>
    {

        private readonly T oldValue;
        private readonly T newValue;
        private bool IsChanged { get => !newValue.Equals(oldValue); }

        public LayoutCallback(T newValue, T oldValue)
        {
            this.newValue = newValue;
            this.oldValue = oldValue;
        }

        public LayoutCallback<T> OnChanged(Action<T, T> action)
        {
            if (IsChanged)
                action(newValue, oldValue);
            return this;
        }

        public LayoutCallback<T> OnChanged(Action<T> action)
        {
            return OnChanged((x, _) => action(x));
        }

        public LayoutCallback<T> OnChanged(Action action)
        {
            return OnChanged(_ => action());
        }

        public LayoutCallback<T> OnBeforeChanged(Action action)
        {
            if (IsChanged)
                action();
            return this;
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

        public static IDisposable Toolbar()
        {
            var disposable = new GUILayout.HorizontalScope("Toolbar", GUILayout.ExpandWidth(true));
            return Disposable.Create(() =>
            {
                disposable.Dispose();
                Space();
            });
        }

        public static bool ToolbarButton(string label)
        {
            return GUILayout.Button(label, "ToolbarButton");
        }

        public static void Space()
        {
            EditorGUILayout.Space();
        }

        public static IDisposable Horizontal()
        {
            var disposable = new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true));
            return Disposable.Create(() =>
            {
                disposable.Dispose();
                Space();
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

        public static LayoutCallback<bool> Toggle<T>(T target, Expression<Func<T, bool>> expression, ActionRecord record = null)
        {
            return Callback(target, expression, x => EditorGUILayout.Toggle(x, GUILayout.Width(15)), record);
        }

        public static LayoutCallback<bool> Foldout<T>(T target, Expression<Func<T, bool>> expression, ActionRecord record = null)
        {
            return Callback(target, expression, x => EditorGUILayout.Toggle(x, new GUIStyle(EditorStyles.foldout), GUILayout.Width(14)), record);
        }


        public static LayoutCallback<float> Float<T>(T target, Expression<Func<T, float>> expression, string label = null, ActionRecord record = null)
        {
            return Callback(target, expression, x => Float(x, label), record);
        }

        public static float Float(float value, string label = null)
        {
            return EditorGUILayout.FloatField(label, value);
        }

        public static LayoutCallback<T> Object<TTarget, T>(TTarget target, Expression<Func<TTarget, T>> expression, string label = null, ActionRecord record = null) where T : UnityEngine.Object
        {
            return Callback(target, expression, x => Object(x, label), record);
        }

        public static T Object<T>(T value = null, string label = null) where T : UnityEngine.Object
        {
            return (T)EditorGUILayout.ObjectField(label, value, typeof(T), true);
        }

        public static LayoutCallback<T> EnumToolbar<TTarget, T>(TTarget target, Expression<Func<TTarget, T>> expression, ActionRecord record = null) where T : Enum
        {
            
            return Callback(target, expression, x => GUILayout.Toolbar(x.GetValue(), EnumHelper.GetValues<T>()
                                                                                            .Select(y => y.ToString())
                                                                                            .ToArray())
                                                           .ToEnum<T>(), record);
        }

        public static LayoutCallback<float> Slider<T>(T target, Expression<Func<T, float>> expression, float leftValue, float rightValue, ActionRecord record = null)
        {
            return Callback(target, expression, x => EditorGUILayout.Slider(x, leftValue, rightValue), record);
        }

        public static LayoutCallback<string> Text<T>(T target, Expression<Func<T, string>> expression, ActionRecord record = null)
        {
            return Callback(target, expression, x => EditorGUILayout.TextField(x), record);
        }

        public static bool Button(string label)
        {
            return GUILayout.Button(label);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }
            
        public static void Label(string label, GUIStyle style = null)
        {
            if (style != null)
            {
                EditorGUILayout.LabelField(label, style);
            }
            else
            {
                EditorGUILayout.LabelField(label);
            }
        }

        private static LayoutCallback<T> Callback<TTarget, T>(TTarget target, Expression<Func<TTarget, T>> expression, Func<T, T> layoutGenerator, ActionRecord record = null)
        {
            //var selector = expression.Compile();
            var oldValue = target.GetValue(expression);
            var newValue = layoutGenerator(oldValue);

            return new LayoutCallback<T>(newValue, oldValue).OnChanged(x =>
            {
                Undo.RecordObject(record.Target, record.ActionName);
                target.SetValue(expression, x);
            });
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
