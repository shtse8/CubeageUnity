using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubeage
{
    [ExecuteInEditMode]
    public class IKController : MonoBehaviour
    {
        public bool enabledIK = true;

        public Transform target;

        [Range(0.0f, 1.0f)]
        public float weight = 1;

        public bool leftHand;
        public bool rightHand;
        public bool leftFoot;
        public bool rightFoot;

        // Start is called before the first frame update
        private void Start()
        {
        }

        public bool IsEnable(AvatarIKGoal goal)
        {
            if (!enabledIK && target != null)
                return false;
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    return leftFoot;
                case AvatarIKGoal.RightFoot:
                    return rightFoot;
                case AvatarIKGoal.LeftHand:
                    return leftHand;
                case AvatarIKGoal.RightHand:
                    return rightHand;
                default:
                    throw new System.Exception("unknown goal");
            }
        }
    }
}