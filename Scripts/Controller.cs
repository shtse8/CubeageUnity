using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;


namespace Cubeage
{
    public class Controller : MonoBehaviour
    {
        [SerializeReference] public GameObject Avatar;
        [SerializeReference] public List<BoneController> BoneControllers = new List<BoneController>();

        void Reset()
        {
            Avatar = gameObject;

        }

        public void AddController()
        {
            BoneControllers.Add(new BoneController(this, $"Controller {BoneControllers.Count + 1}"));
        }

    }
}