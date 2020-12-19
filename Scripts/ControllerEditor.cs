using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Cubeage
{
    [CustomEditor(typeof(Controller))]
    [CanEditMultipleObjects]
    public class ControllerEditor : Editor
    {
        bool showAllValidBones = false;
        string message = "";
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
            /*
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
            */
            /*
            int pickerID = 455454425;
            if (GUILayout.Button("Select"))
            {
                EditorGUIUtility.ShowObjectPicker<Transform>(null, true, "_bc", pickerID);

            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == pickerID)
                {
                    var transform  = EditorGUIUtility.GetObjectPickerObject();
                    Debug.Log(transform);
                }
            }
            */
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
                            {
                                Layout.ObjectLabel(bone);
                                if (controller.TryGetTargetTransform(bone, out var target))
                                {
                                    Layout.ObjectLabel(target);
                                } else
                                {
                                    using (Layout.Color(Color.red))
                                    {
                                        Layout.ObjectLabel<GameObject>(null);
                                    }
                                }
                            }
                        }
                    }
                }

            }

            using (Layout.Toolbar())
            {
                if (Layout.ToolbarButton("Add"))
                {
                    controller.AddController();
                }
                if (Layout.ToolbarButton("Reset"))
                {
                    controller.ResetBones();
                }
                if (Layout.ToolbarButton("Default"))
                {
                    controller.SetToDefault();
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
                              currentController.IsExpanded = x;
                          });
                    Layout.Toggle(currentController.IsEnabled)
                            .OnChanged(x =>
                            {
                                AddUndo("Toggle Controller");
                                currentController.IsEnabled = x;
                            });
                    Layout.Text(currentController.Name)
                            .OnChanged(x =>
                            {
                                AddUndo("Change Controller Name");
                                currentController.Name = x;
                            });
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
                        using (Layout.Toolbar())
                        {
                            if (Layout.ToolbarButton("Reset"))
                            {
                                AddUndo("Reset Controller");
                                currentController.SetToDefault();
                                message = "Reset to Default.";
                            }
                            if (Layout.ToolbarButton("Set Default"))
                            {
                                AddUndo("Set Controller Default");
                                currentController.SetDefault();
                                message = "Set Default Done.";
                            }
                            Layout.FlexibleSpace();
                            Layout.Label(message);
                        }
                        Layout.EnumToolbar(currentController.Mode).OnChanged(x =>
                        {
                            AddUndo("Change Mode");
                            currentController.Mode = x;
                        });

                        Layout.Label($"Bones ({currentController.Bones.Count})");
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
                                Layout.ObjectLabel(bone.Part);
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
                                            Layout.Label(type.ToString(), GUILayout.MinWidth(50));
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
                var value = entry.Value;
                switch (boneController.Mode)
                {
                    case Mode.Min:
                        value = entry.Min;
                        break;
                    case Mode.Max:
                        value = entry.Max;
                        break;
                }
                var floatField = Layout.Float(value, property.Direction.ToString(), minValue, GUILayout.MinWidth(20));
                switch (boneController.Mode)
                {
                    case Mode.Min:
                        floatField.OnChanged(x =>
                            {
                                AddUndo("Change Transform");
                                entry.Min = x;
                            });
                        break;
                    case Mode.Max:
                        floatField.OnChanged(x =>
                            {
                                AddUndo("Change Transform");
                                entry.Max = x;
                            });
                        break;
                }
            }
        }


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
}
