using UnityEngine;
using System;


namespace Cubeage
{
    [Serializable]
    public struct TransformData
    {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Vector3 localEulerAngles;
        public Vector3 position;
        public Vector3 scale;
        public Vector3 eulerAngles;

        public Quaternion rotation;
    }

}