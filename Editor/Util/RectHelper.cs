using UnityEditor;
using UnityEngine;

namespace Cubeage.Avatar.Editor.Util
{

    public class RectHelper
    {
        private Rect _origin;
        private Rect _current;
        private float _spacing;

        public RectHelper(Rect origin, float spacing = 0)
        {
            this._origin = origin;
            this._current = new Rect(origin.x, origin.y, origin.width, 0);
            this._spacing = spacing;
        }

        public void Set(float height)
        {
            this._current.y = this._current.yMax + this._spacing;
            this._current.height = height;
        }

        public Rect Get()
        {
            return this._current;
        }
        public Rect Get(float height)
        {

            this.Set(height);
            return this._current;
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