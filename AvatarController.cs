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
        protected TransformManager manager = new TransformManager();
        public TransformManager Manager => manager;


        #region Avatar
        [SerializeReference]
        [SerializeField]
        private GameObject avatar;

        public GameObject Avatar
        {
            get => avatar;
            set
            {
                if (Equals(avatar, value))
                    return;

                avatar = value;
                manager.Root = avatar.transform;
            }
        }
        #endregion

        public Component RecordTarget => this;


        #region isEnabled
        [SerializeField]
        private bool isEnabled = true;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (Equals(isEnabled, value))
                    return;

                isEnabled = value;

                manager.Update(controllers.Where(x => x.IsEnabled).SelectMany(x => x.BoneControllers), UpdateHints.ToggledEnable);
            }
        }

        #endregion

        #region Controllers
        [SerializeField]
        [SerializeReference]
        private List<Controller> controllers = new List<Controller>();
        public IList<Controller> Controllers => controllers.ToArray();
        #endregion

        public Controller GetController(string name)
        {
            return controllers.First(x => x.Name == name);
        }

        private void Reset()
        {
            Avatar = gameObject;
        }

        #region public methods

        public void Remove(Controller controller)
        {
            controller.IsEnabled = false;
            controllers.Remove(controller);
        }

        public void AddController()
        {
            controllers.Add(new Controller(this, $"Controller {controllers.Count + 1}"));
        }

        public void SetToDefault()
        {
            foreach (var controller in controllers)
            {
                controller.SetToDefault();
            }
        }
        #endregion
    }

}