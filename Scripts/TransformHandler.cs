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
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (Equals(_isExpanded, value))
                    return;

                _isExpanded = value;
            }
        }

        [SerializeField]
        [SerializeReference]
        protected Transform _transform;
        public Transform Transform => _transform;

        [SerializeField]
        protected TransformData _data;
        public TransformData Data => _data;

        [SerializeField]
        [SerializeReference]
        protected List<TransformController> _boneControllers = new List<TransformController>();
        public List<TransformController> BoneControllers => _boneControllers.ToList();

        public IEnumerable<TransformHandler> VirtualParents => _manager.Handlers.Where(x => x.VirtualChildren.Contains(this));

        [SerializeField]
        [SerializeReference]
        protected List<TransformHandler> _virtualChildren = new List<TransformHandler>();
        public List<TransformHandler> VirtualChildren => _virtualChildren.ToList();

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
            if (_transform.name == "LianJia_L_ctrl")
            {
                Debug.Log(GetRelativePosition(_transform.parent, _transform.position).x);
                Debug.Log(_transform.localPosition.x);

            }
            _data = new TransformData
            {
                localPosition = transform.localPosition,
                localEulerAngles = transform.localEulerAngles,
                localScale = transform.localScale,
                position = GetRelativePosition(_manager.Root.transform, transform.position),
                rotation = transform.rotation,
                // scale = transform.lossyScale / _manager.Root.transform.lossyScale
            };

        }

        public static Vector3 GetRelativePosition(Vector3 originPosition, Quaternion originRotation, Vector3 position)
        {
            Vector3 distance = position - originPosition;
            Vector3 relativePosition = Vector3.zero;
            relativePosition.x = Vector3.Dot(distance, (originRotation * Vector3.right).normalized);
            relativePosition.y = Vector3.Dot(distance, (originRotation * Vector3.up).normalized);
            relativePosition.z = Vector3.Dot(distance, (originRotation * Vector3.forward).normalized);
            return relativePosition;
        }

        public static Vector3 GetRelativePosition(Transform origin, Vector3 position)
        {
            Vector3 distance = position - origin.position;
            Vector3 relativePosition = Vector3.zero;
            relativePosition.x = Vector3.Dot(distance, (origin.rotation * Vector3.right).normalized);
            relativePosition.y = Vector3.Dot(distance, (origin.rotation * Vector3.up).normalized);
            relativePosition.z = Vector3.Dot(distance, (origin.rotation * Vector3.forward).normalized);
            // relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
            // relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
            // relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);
            // if (distance != relativePosition)
            // {
            //     Debug.Log(distance.x + ", " + distance.y + ", " + distance.z);
            //     Debug.Log(origin.right.normalized.x + ", " + origin.right.normalized.y + ", " + origin.right.normalized.z);
            //     Debug.Log(relativePosition.x + ", " + relativePosition.y + ", " + relativePosition.z);
            // }
            return relativePosition;
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

        public void AddVirtualChild(Transform transform)
        {
            if (!_manager.TryGet(transform, out var handler))
                throw new Exception("Not a valid child.");
            if (_virtualChildren.Contains(handler))
                throw new Exception("Duplicated child");
            _virtualChildren.Add(handler);
            handler.Update();
        }

        public void RemoveVirtualChild(TransformHandler handler)
        {
            _virtualChildren.Remove(handler);
            handler.Update();
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
            foreach (var entry in VirtualParents.SelectMany(x => x._boneControllers)
                .Where(x => x.TransformVirtualChildren)
                .Select(x => x.Properties[property])
                .Where(x => x.IsOverallEnabled))
            {
                value = GetValue(property.Type, value, entry.Change);
            }

            if (property.Type == TransformType.Position)
            {
                var scaleProperty = new Property(TransformType.Scale, property.Dimension);
                foreach (var controller in VirtualParents.SelectMany(x => x._boneControllers)
                    .Where(x => x.TransformVirtualChildren)
                    .Where(x => x.Properties[scaleProperty].IsOverallEnabled))
                {
                    var offset = GetRelativePosition(controller.TransformHandler._data.position, controller.TransformHandler._data.rotation, _data.position).Get(property.Dimension);
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

            if ((hint == UpdateHints.UpdatedChange || hint == UpdateHints.ToggledEnable) && _boneControllers.Any(x => x.TransformVirtualChildren) || 
                hint == UpdateHints.UpdatedTransformVirtualChildren)
            {
                foreach (var child in VirtualChildren)
                {
                    child.Update(property);
                    if (property.Type == TransformType.Scale)
                    {
                        child.Update(new Property(TransformType.Position, Dimension.X));
                        child.Update(new Property(TransformType.Position, Dimension.Y));
                        child.Update(new Property(TransformType.Position, Dimension.Z));
                    }
                }
            }
        }
        public static Vector3 RotateX(Vector3 vector, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);


            var result = vector;
            result.y = (cos * vector.y) - (sin * vector.z);
            result.z = (cos * vector.z) + (sin * vector.y);

            return result;
        }

        public static Vector3 RotateY(Vector3 vector, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            var result = vector;
            result.x = (cos * vector.x) + (sin * vector.z);
            result.z = (cos * vector.z) - (sin * vector.x);
            return result;
        }

        public static Vector3 RotateZ(Vector3 vector, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            var result = vector;
            result.x = (cos * vector.x) - (sin * vector.y);
            result.y = (cos * vector.y) + (sin * vector.x);
            return result;
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