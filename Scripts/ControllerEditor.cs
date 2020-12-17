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
    [CustomEditor(typeof(Controller))]
    [CanEditMultipleObjects]
    public class ControllerEditor : Editor
    {
        bool showAllValidBones = false;
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
            using (Layout.Horizontal())
            {
                Layout.Object(controller, x => x.Avatar, "Target Avatar")
                    .ApplyChange(() => AddUndo("Set Avatar"));
            }

            using (Layout.Horizontal())
            using (Layout.Box())
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(showAllValidBones).OnChanged(x => showAllValidBones = x);
                    Layout.Label($"Bones: {controller.ValidBones.Count}");
                }
                    
                if (showAllValidBones)
                {
                    using (Layout.Indent())
                    {
                        foreach (var bone in controller.ValidBones.Keys)
                        {
                            using (Layout.Horizontal())
                                Layout.Label(bone.name);
                        }
                    }
                }

            }

            using (Layout.Toolbar())
            {
                if (Layout.ToolbarButton("Add"))
                {
                    AddUndo("Add Controller");
                    controller.AddController();
                }
                if (Layout.ToolbarButton("Debug"))
                {

                    Animator lAnimator = controller.gameObject.GetComponent<Animator>();
                    Debug.Log(lAnimator);

                    Transform lBoneTransform = lAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);

                    Debug.Log(lBoneTransform);
                }
                Layout.FlexibleSpace();
            }

            
            foreach (var currentController in controller.BoneControllers.ToArray())
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(currentController.IsExpanded)
                          .OnChanged(x => {
                              AddUndo("Toggle Controller");
                              currentController.IsExpanded = x;
                          });
                    Layout.Text(currentController, x => x.Name);
                    using (Layout.SetEnable(currentController.Mode == Mode.View))
                    {
                        Layout.Slider(currentController, x => x.Value, 0, 100)
                            .ApplyChange(() => AddUndo("Slide Controller"));
                    }
                    if (DrawRemoveButton())
                    {
                        AddUndo("Remove Bone");
                        controller.BoneControllers.Remove(currentController);
                        continue;
                    }
                }


                if (currentController.IsExpanded)
                {

                    // Self implemented indent
                    using (Layout.Indent())
                    using (Layout.Box())
                    {
                        Layout.EnumToolbar(currentController, x => x.Mode).ApplyChange(() => AddUndo("Change Mode"));

                        Layout.Label($"Bones ({currentController.Bones.Count})", EditorStyles.boldLabel);
                        foreach (var bone in currentController.GetValidBones())
                        {
                            using (Layout.Horizontal())
                            {
                                Layout.Foldout(bone, x => x.isExpanded)
                                      .ApplyChange(() => AddUndo("Expand Bone"));

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
                                                    DrawTransformController(bone, new Property(type, direction), currentController);
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
                            var newControllerPart = Layout.Object<Transform>();
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

        void AddUndo(string name)
        {
            Undo.RecordObject(controller, name);
        }

        void AddUndo(UnityEngine.Object target, string name)
        {
            Undo.RecordObject(target, name);
        }
        void AddUndo(UnityEngine.Object[] targets, string name)
        {
            Undo.RecordObjects(targets, name);
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
        void DrawTransformController(Bone bone, Property property, BoneController boneController)
        {
            var entry = bone.Properties[property];
            if (boneController.Mode == Mode.View)
            {
                using (Layout.SetEnable(entry.IsEnabled || bone.IsAvailable(property)))
                {
                    Layout.Toggle(entry.IsEnabled)
                          .OnChanged(x =>
                          {
                              AddUndo("Toggle Property");
                              entry.IsEnabled = x;
                          });
                }
            }

            using (Layout.SetEnable(boneController.Mode != Mode.View && entry.IsEnabled))
            {
                float? minValue = null;
                if (property.Type == TransformType.Scale)
                    minValue = 0.01f;
                switch (boneController.Mode)
                {
                    case Mode.Min:
                        
                        Layout.Float(entry, x => x.Min, property.Direction.ToString(), minValue)
                            .OnChanged(x =>
                            {
                                AddUndo("Change Transform");
                                entry.Min = x;
                            });
                        break;
                    case Mode.Max:
                        Layout.Float(entry, x => x.Max, property.Direction.ToString(), minValue)
                            .OnChanged(x =>
                            {
                                AddUndo("Change Transform");
                                entry.Max = x;
                            });
                        break;
                    case Mode.View:
                        Layout.Float(entry.Value, property.Direction.ToString());
                        break;
                }
            }
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

    public class LayoutPromise<TTarget, TValue>
    {

        private TTarget Target { get; }
        private Expression<Func<TTarget, TValue>> Expression { get; }
        private TValue OldValue { get; set; }
        private TValue NewValue { get; set; }

        public LayoutPromise(TTarget target, Expression<Func<TTarget, TValue>> expression, Func<TValue, TValue> func)
        {
            Target = target;
            Expression = expression;
            OldValue = target.GetValue(expression);
            NewValue = func(OldValue);
        }


        public LayoutPromise<TTarget, TValue> OnChanged(Action<TValue, TValue> action)
        {
            if (!NewValue.Equals(OldValue))
                action(NewValue, OldValue);
            return this;
        }

        public LayoutPromise<TTarget, TValue> OnChanged(Action<TValue> action)
        {
            return OnChanged((x, _) => action(x));
        }

        public LayoutPromise<TTarget, TValue> OnChanged(Action action)
        {
            return OnChanged(_ => action());
        }

        public LayoutPromise<TTarget, TValue> ApplyChange()
        {
            if (!NewValue.Equals(OldValue))
                Target.SetValue(Expression, NewValue);
            return this;
        }

        public LayoutPromise<TTarget, TValue> ApplyChange(Action action)
        {
            return OnChanged(action).ApplyChange();
        }
        public LayoutPromise<TTarget, TValue> ApplyChange(Action<TValue> action)
        {
            return OnChanged(action).ApplyChange();
        }
        public LayoutPromise<TTarget, TValue> ApplyChange(Action<TValue, TValue> action)
        {
            return OnChanged(action).ApplyChange();
        }

    }

    public class LayoutPromise<TValue>
    {

        private TValue OldValue { get; set; }
        private TValue NewValue { get; set; }

        public LayoutPromise(TValue oldValue, Func<TValue, TValue> func)
        {
            OldValue = oldValue;
            NewValue = func(OldValue);
        }


        public LayoutPromise<TValue> OnChanged(Action<TValue, TValue> action)
        {
            if (!NewValue.Equals(OldValue))
                action(NewValue, OldValue);
            return this;
        }

        public LayoutPromise<TValue> OnChanged(Action<TValue> action)
        {
            return OnChanged((x, _) => action(x));
        }

        public LayoutPromise<TValue> OnChanged(Action action)
        {
            return OnChanged(_ => action());
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

        public static bool Button(string label)
        {
            return GUILayout.Button(label);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }


        public static LayoutPromise<bool> Toggle(bool value)
        {
            return new LayoutPromise<bool>(value, x => EditorGUILayout.Toggle(x, GUILayout.Width(15)));
        }

        public static LayoutPromise<T, bool> Toggle<T>(T target, Expression<Func<T, bool>> expression)
        {
            return new LayoutPromise<T, bool>(target, expression, x => EditorGUILayout.Toggle(x, GUILayout.Width(15)));
        }

        public static LayoutPromise<T, bool> Foldout<T>(T target, Expression<Func<T, bool>> expression)
        {
            return new LayoutPromise<T, bool>(target, expression, x => EditorGUILayout.Toggle(x, new GUIStyle(EditorStyles.foldout), GUILayout.Width(14)));
        }


        public static LayoutPromise<bool> Foldout(bool value)
        {
            return new LayoutPromise<bool>(value, x => EditorGUILayout.Toggle(x, new GUIStyle(EditorStyles.foldout), GUILayout.Width(14)));
        }

        public static LayoutPromise<T, float> Float<T>(T target, Expression<Func<T, float>> expression, string label = null, float? minValue = null)
        {
            return new LayoutPromise<T, float>(target, expression, x =>
            {
                var value = Float(x, label);
                if (minValue.HasValue)
                    value = Math.Max(minValue.Value, value);
                return value;
            });
        }


        // Need refactor
        public static float Float(float value, string label = null)
        {
            return EditorGUILayout.FloatField(label, value);
        }

        public static T Object<T>(T value = null, string label = null) where T : UnityEngine.Object
        {
            return (T)EditorGUILayout.ObjectField(label, value, typeof(T), true);
        }

        public static LayoutPromise<TTarget, TValue> Object<TTarget, TValue>(TTarget target, Expression<Func<TTarget, TValue>> expression, string label = null) where TValue : UnityEngine.Object
        {
            return new LayoutPromise<TTarget, TValue>(target, expression, x => Object(x, label));
        }

        public static LayoutPromise<TTarget, TValue> EnumToolbar<TTarget, TValue>(TTarget target, Expression<Func<TTarget, TValue>> expression) where TValue : Enum
        {
            
            return new LayoutPromise<TTarget, TValue>(target, expression, x => GUILayout.Toolbar(x.GetValue(), EnumHelper.GetValues<TValue>()
                                                                                            .Select(y => y.ToString())
                                                                                            .ToArray())
                                                           .ToEnum<TValue>());
        }

        public static LayoutPromise<T, float> Slider<T>(T target, Expression<Func<T, float>> expression, float leftValue, float rightValue)
        {
            return new LayoutPromise<T, float>(target, expression, x => EditorGUILayout.Slider(x, leftValue, rightValue));
        }

        public static LayoutPromise<T, string> Text<T>(T target, Expression<Func<T, string>> expression)
        {
            return new LayoutPromise<T, string>(target, expression, x => EditorGUILayout.TextField(x));
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

    }
}
