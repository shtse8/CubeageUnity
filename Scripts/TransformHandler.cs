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

        public IEnumerable<TransformHandler> Siblings
        {
            get
            {
                var parent = _transform.parent;
                for (var i = 0; i < parent.childCount; i++)
                {
                    var child = parent.GetChild(i);
                    if (Equals(child, _transform))
                        continue;
                    if (_manager.TryGet(child, out var handler))
                        yield return handler;
                }
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
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (_manager.TryGet(child, out var handler))
                    yield return handler;
                else
                    foreach(var x in GetChildren(child))
                        yield return x;
            }
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
            Update(controller, UpdateHints.ToggledEnable);
        }

        public void AddTransformController(TransformController controller)
        {
            if (_boneControllers.Contains(controller))
                throw new Exception("Duplicated bone controller.");
            _boneControllers.Add(controller);
            Update(controller, UpdateHints.ToggledEnable);
        }

        public bool IsValid()
        {
            return _transform;
        }


        public void Update(UpdateHints? hint = null)
        {
            foreach (var property in Property.GetAll())
            {
                Update(property, hint);
            }
        }

        public void Update(TransformController boneController, UpdateHints? hint = null)
        {
            Update(boneController.Yield(), hint);
        }

        public void Update(IEnumerable<TransformController> boneControllers, UpdateHints? hint = null)
        {
            // check which properties should be updated.
            var targetProperties = boneControllers.Intersect(_boneControllers)
                                                  .SelectMany(x => x.Properties.Values)
                                                  .Where(x => hint == UpdateHints.ToggledEnable ? x.IsEnabled : x.IsOverallEnabled)
                                                  .Select(x => x.Property);
            foreach (var property in Property.GetAll().Intersect(targetProperties))
            {
                Update(property, hint);
            }
        }


        public void Update(Property property, UpdateHints? hint = null)
        {
            var value = _data.Get(property);
            foreach (var entry in _boneControllers
                    .Select(x => x.Properties[property])
                    .Where(x => x.IsOverallEnabled))
            {
                value = GetValue(property.Type, value, entry.Change);
            }
            

            // Siblings
            foreach (var entry in Siblings.SelectMany(x => x._boneControllers)
                .Where(x => x.TransformSiblings)
                .Select(x => x.Properties[property])
                .Where(x => x.IsOverallEnabled))
            {
                value = GetValue(property.Type, value, entry.Change);
            }

            if (property.Type == TransformType.Position)
            {
                var scaleProperty = new Property(TransformType.Scale, property.Dimension);
                foreach (var controller in Siblings.SelectMany(x => x._boneControllers)
                    .Where(x => x.TransformSiblings)
                    .Where(x => x.Properties[scaleProperty].IsOverallEnabled))
                {
                    var origin = controller.TransformHandler._data.Get(property);
                    var offset = _data.Get(property) - origin;
                    var scaledOffset = offset * (controller.Properties[scaleProperty].Change - 1);
                    value = GetValue(property.Type, value, scaledOffset);
                }
            }

            // Parent
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

            if ((hint == UpdateHints.UpdatedChange || hint == UpdateHints.ToggledEnable) && _boneControllers.Any(x => x.TransformChildren) ||
                hint == UpdateHints.UpdatedTransformChildren)
            {
                foreach (var child in Children)
                {
                    child.Update(property);
                }
            }

            if ((hint == UpdateHints.UpdatedChange || hint == UpdateHints.ToggledEnable) && _boneControllers.Any(x => x.TransformSiblings) || 
                hint == UpdateHints.UpdatedTransformSiblings)
            {
                foreach (var sibling in Siblings)
                {
                    sibling.Update(property);
                    if (property.Type == TransformType.Scale)
                        sibling.Update(new Property(TransformType.Position, property.Dimension));
                }
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