using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;


namespace Cubeage
{

    [AddComponentMenu("Cubeage/Avatar Controller")]
    public class AvatarController : MonoBehaviour
    {
        [SerializeReference]
        [SerializeField]
        protected TransformManager _manager = new TransformManager();
        public TransformManager Manager => _manager;


        #region Avatar
        [SerializeReference]
        [SerializeField]
        private GameObject _avatar;

        public GameObject Avatar
        {
            get => _avatar;
            set
            {
                if (Equals(_avatar, value))
                    return;

                _avatar = value;
                _manager.Build(_avatar.transform);
            }
        }
        #endregion

        public Component RecordTarget => this;


        #region isEnabled
        [SerializeField]
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (Equals(_isEnabled, value))
                    return;

                _isEnabled = value;

                _manager.Update();
            }
        }

        #endregion

        #region Controllers
        [SerializeField]
        [SerializeReference]
        private List<Controller> _controllers = new List<Controller>();
        public IList<Controller> Controllers => _controllers.ToArray();
        #endregion

        void Reset()
        {
            Avatar = gameObject;
        }

        #region public methods

        public void Remove(Controller controller)
        {
            Undo.RecordObject(RecordTarget, "Remove Controller");
            controller.IsEnabled = false;
            _controllers.Remove(controller);
        }

        public void AddController()
        {
            Undo.RecordObject(RecordTarget, "Add Controller");
            _controllers.Add(new Controller(this, $"Controller {_controllers.Count + 1}"));
        }

        public void SetToDefault()
        {
            Undo.RecordObject(RecordTarget, "Set All Controller To Default");
            foreach (var controller in _controllers)
            {
                controller.SetToDefault();
            }
        }
        #endregion
    }

}