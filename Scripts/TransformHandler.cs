using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


namespace Cubeage
{
    [Serializable]
    public class TransformHandler
    {
        public TransformHandler Parent
        {
            get
            {
                var parent = _transform.parent;
                while (parent)
                {
                    if (_manager.TryGet(parent, out var handler))
                        return handler;
                    parent = parent.parent;
                }
                return null;
            }
        }

        public IEnumerable<TransformHandler> Children => GetChildren(_transform);

        [SerializeField]
        [SerializeReference]
        protected Transform _transform;
        public Transform Transform => _transform;

        [SerializeField]
        protected TransformData _data;

        [SerializeField]
        [SerializeReference]
        protected List<TransformController> _boneControllers = new List<TransformController>();
        public List<TransformController> BoneControllers => _boneControllers.ToList();

        [SerializeField]
        [SerializeReference]
        protected TransformManager _manager;

        IEnumerable<TransformHandler> GetChildren(Transform transform)
        {
            var handlers = new List<TransformHandler>();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (_manager.TryGet(child, out var handler))
                    handlers.Add(handler);
                else
                    handlers.AddRange(GetChildren(child));
            }
            return handlers;
        }

        public TransformHandler(TransformManager manager, Transform transform)
        {
            _manager = manager;
            _transform = transform;
            _data = new TransformData
            {
                localPosition = transform.localPosition,
                localEulerAngles = transform.localEulerAngles,
                localScale = transform.localScale
            };

        }


        public TransformController CreateTransformController(Controller controller)
        {
            var boneController = new TransformController(controller, this);
            AddTransformController(boneController);
            return boneController;
        }

        public void RemoveTransformController(TransformController controller)
        {
            _boneControllers.Remove(controller);
            Update();
        }

        public void AddTransformController(TransformController controller)
        {
            if (_boneControllers.Contains(controller))
                throw new Exception("Duplicated bone controller.");
            _boneControllers.Add(controller);
            Update();
        }

        public bool IsValid()
        {
            return _transform;
        }


        public void Update()
        {
            foreach (var property in Property.GetAll())
            {
                Update(property);
            }
        }

        public void Update(TransformController boneController)
        {
            foreach (var property in boneController.Properties.Values.Where(x => x.IsEnabled).Select(x => x.Property))
            {
                Update(property);
            }
        }

        public void Update(Property property)
        {
            var value = _data.Get(property);
            foreach (var entry in _boneControllers
                    .Select(x => x.Properties[property])
                    .Where(x => x.IsOverallEnabled))
            {
                value = GetValue(property.Type, value, entry.Change);
            }

            // Find Parent Controller
            foreach (var entry in Parent?._boneControllers
                .Where(x => !x.TransformChildren)
                .Select(x => x.Properties[property])
                .Where(x => x.IsOverallEnabled))
            {
                value = GetValue(property.Type, value, GetCounterChange(property.Type, entry.Change));
            }

            if (!Equals(_transform.Get(property), value))
            {
                _transform.Set(property, value);
            }

            // Update Children
            foreach (var child in Children)
            {
                child.Update(property);
            }
        }

        float GetCounterChange(TransformType type, float change)
        {
            switch (type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return -change;
                case TransformType.Scale:
                    return 1 / change;
                default:
                    throw new Exception("Unknown Type.");
            }
        }

        float GetValue(TransformType type, float value, float change)
        {
            switch (type)
            {
                case TransformType.Position:
                case TransformType.Rotation:
                    return value + change;
                case TransformType.Scale:
                    return value * change;
                default:
                    throw new Exception("Unknown Type.");
            }
        }

        public bool TryGetTargetTransform(out Transform target)
        {
            target = null;
            for (var i = 0; i < _transform.childCount; i++)
            {
                var child = _transform.GetChild(i);
                if (Equals(child.name + _manager.Suffix, _transform.name))
                {
                    target = child;
                    return true;
                }
            }
            return false;
        }

    }

}