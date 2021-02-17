using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

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
                _manager.Root = _avatar.transform;
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

                _manager.Update(_controllers.Where(x => x.IsEnabled).SelectMany(x => x.BoneControllers), UpdateHints.ToggledEnable);
            }
        }

        #endregion

        #region Controllers
        [SerializeField]
        [SerializeReference]
        private List<Controller> _controllers = new List<Controller>();
        public IList<Controller> Controllers => _controllers.ToArray();
        #endregion

        public Controller GetController(string name)
        {
            return _controllers.First(x => x.Name == name);
        }

        void Reset()
        {
            Avatar = gameObject;
        }

        #region public methods

        public void Remove(Controller controller)
        {
            controller.IsEnabled = false;
            _controllers.Remove(controller);
        }

        public void AddController()
        {
            _controllers.Add(new Controller(this, $"Controller {_controllers.Count + 1}"));
        }

        public void SetToDefault()
        {
            foreach (var controller in _controllers)
            {
                controller.SetToDefault();
            }
        }
        #endregion
    }

}