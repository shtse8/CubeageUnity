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
    public class ControllerEditor : Editor
    {
        bool _showAllValidBones = false;
        string message = "";
        Controller _controller;

        void OnEnable()
        {
            _controller = (Controller)target;
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
            }) && ConfirmRemove();
        }

        bool ConfirmRemove()
        {
            return Confirm("Are you sure want to remove?");
        }

        public override void OnInspectorGUI()
        {
            // using (Layout.Horizontal())
            // {
            //     Layout.Label("Target Avatar");
            //     Layout.Object(_controller.Avatar)
            //         .OnChanged(x =>
            //         {
            //             _controller.Avatar = x;
            //         });
            // }
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
                    Layout.Foldout(_showAllValidBones).OnChanged(x => _showAllValidBones = x);
                    Layout.Label($"Bones: {_controller.ValidBones.Count}");
                }
                    
                if (_showAllValidBones)
                {
                    using (Layout.Indent())
                    {
                        foreach (var bone in _controller.ValidBones)
                        {
                            using (Layout.Horizontal())
                            {
                                Layout.ObjectLabel(bone);
                                if (_controller.TryGetTargetTransform(bone, out var target))
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
                if (Layout.ToolbarButton("Add Controller"))
                {
                    _controller.AddController();
                }
                if (Layout.ToolbarButton("Set All To Default"))
                {
                    _controller.SetToDefault();
                }
                Layout.FlexibleSpace();
                if (Layout.ToolbarButton("Fix"))
                {
                    _controller.ResetBones();
                }
                // if (Layout.ToolbarButton("測試"))
                // {
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
                // }
            }

            
            foreach (var currentController in _controller.BoneControllers)
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
                                currentController.IsEnabled = x;
                            });
                    Layout.Text(currentController.Name)
                            .OnChanged(x =>
                            {
                                currentController.Name = x;
                            });
                    Layout.Slider(currentController.Value, 0, 100)
                        .OnChanged(x =>
                        {
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
                                currentController.SetToDefault();
                            }
                            if (Layout.ToolbarButton("Set Default"))
                            {
                                currentController.SetDefault();
                            }
                            Layout.FlexibleSpace();

                            if (Layout.ToolbarButton("Remove") && ConfirmRemove())
                            {
                                _controller.Remove(currentController);
                                GUIUtility.ExitGUI();
                            }
                        }
                        Layout.EnumToolbar(currentController.Mode).OnChanged(x =>
                        {
                            currentController.Mode = x;
                        });

                        Layout.Label($"Bones ({currentController.Bones.Count})");
                        foreach (var bone in currentController.Bones)
                        {
                            using (Layout.Horizontal())
                            {
                                Layout.Foldout(bone.IsExpanded)
                                    .OnChanged(x =>
                                    {
                                        bone.IsExpanded = x;
                                    });

                                Layout.Toggle(bone.IsEnabled)
                                        .OnChanged(x =>
                                        {
                                            bone.IsEnabled = x;
                                        });
                                Layout.ObjectLabel(bone.Transform);
                                if (Layout.MiniButton("Remove") && ConfirmRemove())
                                {
                                    currentController.Remove(bone);
                                    GUIUtility.ExitGUI();
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

                            if (bone.IsExpanded)
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
                                                foreach (var direction in EnumHelper.GetValues<Dimension>())
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


        bool Confirm(string message)
        {
            return EditorUtility.DisplayDialog("Controller @ Cubeage", message, "Yes", "No");
        }

        void Alert(string message)
        {
            EditorUtility.DisplayDialog("Controller @ Cubeage", message, "Okay");
        }

        void DrawTransformController(Bone bone, Property property, BoneController boneController)
        {
            var entry = bone.Properties[property];
            using (Layout.SetEnable(entry.IsEnabled || bone.IsAvailable(property)))
            {
                Layout.Toggle(entry.IsEnabled)
                        .OnChanged(x =>
                        {
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
                var floatField = Layout.Float(value, property.Dimension.ToString(), minValue, GUILayout.MinWidth(20));
                switch (boneController.Mode)
                {
                    case Mode.Min:
                        floatField.OnChanged(x =>
                            {
                                entry.Min = x;
                            });
                        break;
                    case Mode.Max:
                        floatField.OnChanged(x =>
                            {
                                entry.Max = x;
                            });
                        break;
                }
            }
        }


    }
}
