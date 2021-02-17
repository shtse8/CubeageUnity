using UnityEditor;
using UnityEngine;

namespace Cubeage.Avatar.Editor.Util
{

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
}