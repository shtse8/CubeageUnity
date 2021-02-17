using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cubeage
{
    [ExecuteInEditMode]
    public class IKControllerHost : MonoBehaviour
    {
        public Animator animator;
        public IKController[] controllers;

        // Start is called before the first frame update
        private void Start()
        {
            animator = gameObject.GetComponent<Animator>();
        }

        // Update is called once per frame
        private void Update()
        {
            animator.Update(0);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            var dict = EnumHelper.GetValues<AvatarIKGoal>().ToDictionary(x => x, x => false);
            if (animator)
            {
                foreach (var controller in controllers) 
                {
                    foreach (var goal in EnumHelper.GetValues<AvatarIKGoal>())
                    {
                        if (controller.IsEnable(goal))
                        {
                            dict[goal] = true;
                            animator.SetIKPositionWeight(goal, controller.weight);
                            animator.SetIKRotationWeight(goal, controller.weight);
                            if (controller.target != null)
                            {
                                animator.SetIKPosition(goal, controller.target.position);
                                animator.SetIKRotation(goal, controller.target.rotation);
                            }
                        }
                    }
                }
            }

            foreach(var goal in dict.Where(x => !x.Value).Select(x => x.Key))
            {
                animator.SetIKPositionWeight(goal, 0);
                animator.SetIKRotationWeight(goal, 0);
            }
        }


    }
}