using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Cubeage
{
    [CustomEditor(typeof(AvatarController))]
    public class ControllerEditor : Editor
    {
        bool _showAllValidBones = false;
        string message = "";
        AvatarController _avatarController;

        void OnEnable()
        {
            _avatarController = (AvatarController)target;
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
            using (Layout.Horizontal())
            {
                Layout.Label("Enable");
                Layout.Toggle(_avatarController.IsEnabled)
                    .OnChanged(x =>
                    {
                        _avatarController.IsEnabled = x;
                    });
            }
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
                    Layout.Label($"Bones: {_avatarController.Manager.Handlers.Count}");
                }

                if (_showAllValidBones)
                {
                    using (Layout.Indent())
                    {
                        foreach (var handler in _avatarController.Manager.Handlers)
                        {
                            using (Layout.Horizontal())
                            {
                                Layout.ObjectLabel(handler.Transform);
                                if (handler.TryGetTargetTransform(out var target))
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
                    _avatarController.AddController();
                }
                if (Layout.ToolbarButton("Set All To Default"))
                {
                    _avatarController.SetToDefault();
                }
                Layout.FlexibleSpace();
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

            
            foreach (var controller in _avatarController.Controllers)
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(controller.IsExpanded)
                          .OnChanged(x => {
                              controller.IsExpanded = x;
                          });
                    Layout.Toggle(controller.IsEnabled)
                            .OnChanged(x =>
                            {
                                controller.IsEnabled = x;
                            });
                    Layout.Text(controller.Name)
                            .OnChanged(x =>
                            {
                                controller.Name = x;
                            });
                    Layout.Slider(controller.Value, 0, 100)
                        .OnChanged(x =>
                        {
                            controller.Value = x;
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


                if (controller.IsExpanded)
                {
                    using (Layout.Indent())
                    using (Layout.Box())
                    {
                        using (Layout.Toolbar())
                        {
                            if (Layout.ToolbarButton("Reset"))
                            {
                                controller.SetToDefault();
                            }
                            if (Layout.ToolbarButton("Set Default"))
                            {
                                controller.SetDefault();
                            }
                            Layout.FlexibleSpace();

                            if (Layout.ToolbarButton("Remove") && ConfirmRemove())
                            {
                                _avatarController.Remove(controller);
                                GUIUtility.ExitGUI();
                            }
                        }
                        Layout.EnumToolbar(controller.Mode).OnChanged(x =>
                        {
                            controller.Mode = x;
                        });


                        Layout.Label($"Bones ({controller.BoneControllers.Count})");
                        foreach (var bone in controller.BoneControllers)
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
                                Layout.Object(bone.Transform).OnChanged(x =>
                                {
                                    bone.Transform = x;
                                });
                                if (Layout.MiniButton("Remove") && ConfirmRemove())
                                {
                                    controller.Remove(bone);
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
                                    using (Layout.Horizontal())
                                    {
                                        Layout.Label("Transform", GUILayout.MinWidth(50));
                                        Layout.FlexibleSpace();
                                        Layout.Toggle(bone.TransformChildren)
                                                .OnChanged(x =>
                                                {
                                                    bone.TransformChildren = x;
                                                });
                                        Layout.Label("Children", GUILayout.MinWidth(50), GUILayout.MaxWidth(120));
                                        Layout.Toggle(bone.TransformSiblings)
                                                .OnChanged(x =>
                                                {
                                                    bone.TransformSiblings = x;
                                                });
                                        Layout.Label("Siblings", GUILayout.MinWidth(50), GUILayout.MaxWidth(120));
                                    }

                                    foreach (var type in EnumHelper.GetValues<TransformType>())
                                    {
                                        using (Layout.Horizontal())
                                        {
                                            Layout.Label(type.ToString(), GUILayout.MinWidth(50));
                                            using (Layout.SetLabelWidth(10))
                                            {
                                                foreach (var direction in EnumHelper.GetValues<Dimension>())
                                                {
                                                    DrawTransformController(bone, new Property(type, direction), controller);
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
                                        controller.Add(x);
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

        void DrawTransformController(TransformController bone, Property property, Controller boneController)
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
