using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace Cubeage
{
    public class Layout
    {


        public static IDisposable SetEnable(bool isEnabled)
        {
            var oldValue = GUI.enabled;
            GUI.enabled = isEnabled;
            return Disposable.Create(() => GUI.enabled = oldValue);
        }

        public static IDisposable Color(Color color)
        {
            var oldValue = GUI.contentColor;
            GUI.contentColor = color;
            return Disposable.Create(() => GUI.contentColor = oldValue);
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

        public static IDisposable Vertical()
        {
            var disposable = new GUILayout.VerticalScope();
            return Disposable.Create(() =>
            {
                disposable.Dispose();
            });
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

        public static bool MiniButton(string label)
        {
            return GUILayout.Button(label, EditorStyles.miniButton);
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

        public static LayoutPromise<float> Float(float value, string label = null, float? minValue = null, params GUILayoutOption[] options)
        {
            return new LayoutPromise<float>(value, x =>
            {
                var newValue = EditorGUILayout.FloatField(label, x, options);
                if (minValue.HasValue)
                    newValue = Math.Max(minValue.Value, newValue);
                return newValue;
            });
        }

        private static double lastClickTime;
        public static void ObjectLabel<T>(T value) where T : UnityEngine.Object
        {
            if (GUILayout.Button(EditorGUIUtility.ObjectContent(value, typeof(T)), new GUIStyle(EditorStyles.textField)
            {
                fixedHeight = EditorGUIUtility.singleLineHeight,
                imagePosition = value ? ImagePosition.ImageLeft : ImagePosition.TextOnly
            }) && value)
            {
                if (EditorApplication.timeSinceStartup - lastClickTime < 0.3)
                {
                    if (value is Component component)
                    {
                        Selection.activeGameObject = component.gameObject;
                        SceneView.FrameLastActiveSceneView();
                    }
                } else
                {
                    EditorGUIUtility.PingObject(value);
                    lastClickTime = EditorApplication.timeSinceStartup;
                }
            }
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

        public static void Label(string label, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(label, options);
        }
    }
}
