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
                hover = new GUIStyleState()
                {
                    textColor = Color.red
                }
            }) && Confirm("Are you sure want to remove?");
        }


        public override void OnInspectorGUI()
        {
            using (Layout.Horizontal())
            {
                Layout.Label("Target Avatar");
                Layout.Object(controller.Avatar)
                    .OnChanged(x =>
                    {
                        AddUndo("Set Avatar");
                        controller.Avatar = x;
                    });
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
                if (Layout.ToolbarButton("Reset"))
                {
                    controller.ResetBones();
                }
                if (Layout.ToolbarButton("Debug"))
                {
                    // foreach(var x  in controller.BoneControllers[0].Bones[0].Properties)
                    // {
                    //     Debug.Log(x.Key, x.Value);
                    // }
                    // Animator lAnimator = controller.gameObject.GetComponent<Animator>();
                    // Debug.Log(lAnimator);
                    // 
                    // Transform lBoneTransform = lAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    // 
                    // Debug.Log(lBoneTransform);
                }
                Layout.FlexibleSpace();
            }

            
            foreach (var currentController in controller.BoneControllers.ToArray())
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(currentController.IsExpanded)
                          .OnChanged(x => {
                              AddUndo("Expand Controller");
                              currentController.IsExpanded = x;
                          });
                    Layout.Toggle(currentController.IsEnabled)
                            .OnChanged(x =>
                            {
                                AddUndo("Toggle Controller");
                                currentController.IsEnabled = x;
                            });
                    Layout.Text(currentController.Name);
                    Layout.Slider(currentController.Value, 0, 100)
                        .OnChanged(x =>
                        {
                            AddUndo("Slide Controller");
                            currentController.Value = x;
                        });
                    /*
                    if (DrawRemoveButton())
                    {
                        AddUndo("Remove Bone");
                        controller.BoneControllers.Remove(currentController);
                        continue;
                    }
                    */
                }


                if (currentController.IsExpanded)
                {
                    using (Layout.Indent())
                    using (Layout.Box())
                    {
                        Layout.EnumToolbar(currentController.Mode).OnChanged(x =>
                        {
                            AddUndo("Change Mode");
                            currentController.Mode = x;
                        });

                        Layout.Label($"Bones ({currentController.Bones.Count})", EditorStyles.boldLabel);
                        foreach (var bone in currentController.GetValidBones())
                        {
                            using (Layout.Horizontal())
                            {
                                Layout.Foldout(bone.isExpanded)
                                    .OnChanged(x =>
                                    {
                                        AddUndo("Expand Bone");
                                        bone.isExpanded = x;
                                    });

                                Layout.Toggle(bone.IsEnabled)
                                        .OnChanged(x =>
                                        {
                                            AddUndo("Toggle Bone");
                                            bone.IsEnabled = x;
                                        });
                                using (Layout.SetEnable(false))
                                {
                                    Layout.Object(bone.Part);
                                }
                                /*
                                if (DrawRemoveButton())
                                {
                                    AddUndo("Remove Bone");
                                    currentController.Remove(bone);
                                    continue;
                                }
                                */
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

                        using (Layout.Horizontal())
                        {
                            Layout.Space(32);
                            Layout.Object<Transform>(null).OnChanged(x =>
                            {
                                if (x != null)
                                {
                                    try
                                    {
                                        AddUndo("Add Bone");
                                        currentController.Add(x);
                                    }
                                    catch (Exception e)
                                    {
                                        Alert(e.Message);
                                    }
                                }
                            });
                        }


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

        void AddUndo(string name)
        {
            Undo.SetCurrentGroupName(name);
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
            using (Layout.SetEnable(entry.IsEnabled || bone.IsAvailable(property)))
            {
                Layout.Toggle(entry.IsEnabled)
                        .OnChanged(x =>
                        {
                            AddUndo("Toggle Property");
                            entry.IsEnabled = x;
                        });
            }

            using (Layout.SetEnable(boneController.Mode != Mode.View && entry.IsEnabled))
            {
                float? minValue = null;
                if (property.Type == TransformType.Scale)
                    minValue = 0.01f;
                if (entry.IsEnabled)
                {
                    switch (boneController.Mode)
                    {
                        case Mode.Min:

                            Layout.Float(entry.Min, property.Direction.ToString(), minValue)
                                .OnChanged(x =>
                                {
                                    AddUndo("Change Transform");
                                    entry.Min = x;
                                });
                            break;
                        case Mode.Max:
                            Layout.Float(entry.Max, property.Direction.ToString(), minValue)
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
                else
                {
                    Layout.Float(entry.Value, property.Direction.ToString());
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
            if (!Equals(NewValue, OldValue))
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
            var disposable = new GUILayout.HorizontalScope();
            return Disposable.Create(() =>
            {
                disposable.Dispose();
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

        public static LayoutPromise<bool> Foldout(bool value)
        {
            return new LayoutPromise<bool>(value, x => EditorGUILayout.Toggle(x, new GUIStyle(EditorStyles.foldout), GUILayout.Width(14)));
        }

        public static void Space(float width)
        {
            // EditorGUILayout.Space(width, false);
            EditorGUILayout.LabelField("", GUILayout.Width(width));
        }

        public static LayoutPromise<float> Float(float value, string label = null, float? minValue = null)
        {
            return new LayoutPromise<float>(value, x =>
            {
                var newValue = EditorGUILayout.FloatField(label, x);
                if (minValue.HasValue)
                    newValue = Math.Max(minValue.Value, newValue);
                return newValue;
            });
        }


        public static LayoutPromise<T> Object<T>(T value) where T : UnityEngine.Object
        {
            
            return new LayoutPromise<T>(value, x => (T)EditorGUILayout.ObjectField(value, typeof(T), true));
        }

        public static LayoutPromise<T> EnumToolbar<T>(T value) where T : Enum
        {
            
            return new LayoutPromise<T>(value, x => GUILayout.Toolbar(x.GetValue(), EnumHelper.GetValues<T>()
                                                                                            .Select(y => y.ToString())
                                                                                            .ToArray())
                                                           .ToEnum<T>());
        }

        public static LayoutPromise<float> Slider(float value, float leftValue, float rightValue)
        {
            return new LayoutPromise<float>(value, x => EditorGUILayout.Slider(x, leftValue, rightValue));
        }

        public static LayoutPromise<string> Text(string value)
        {
            return new LayoutPromise<string>(value, x => EditorGUILayout.TextField(x));
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
