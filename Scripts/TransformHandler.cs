﻿using System.Collections.Generic;
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
                var current = _transform.parent;
                while (current)
                {
                    if (_manager.TryGet(current, out var handler))
                        return handler;
                    current = current.parent;
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

        public string Name => _transform?.name;

        #region Virtual Hierarchy
        [SerializeField]
        [SerializeReference]
        protected TransformHandler _virtualParent;
        public TransformHandler VirtualParent
        {
            get => _virtualParent;
            set
            {
                if (Equals(_virtualParent, value))
                    return;

                if (Equals(this, value))
                    throw new Exception("Cannot set itself as parent.");

                _virtualParent = value;
                Update();
            }
        }

        public IEnumerable<TransformHandler> VirtualChildren => _manager.Handlers.Where(x => Equals(x._virtualParent, this));
        #endregion

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

        public void SetVirtualParent(Transform transform)
        {
            if (!_manager.TryGet(transform, out var handler))
                throw new Exception("Not a valid parent.");
            VirtualParent = handler;
        }

        public void AddVirtualChild(Transform transform)
        {
            if (!_manager.TryGet(transform, out var handler))
                throw new Exception("Not a valid child.");
            Debug.Log(handler.VirtualParent.Transform.name);
            handler.VirtualParent = this;
            Debug.Log(handler.VirtualParent.Transform.name);
        }

        public bool IsValid()
        {
            return _transform;
        }


        public void Update(UpdateHints? hint = null)
        {
            Update(Property.GetAll().SelectMany(x => GenerateUpdateRequest(x, hint)));
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
            Update(Property.GetAll().Intersect(targetProperties).SelectMany(x => GenerateUpdateRequest(x, hint)));
        }


        public void Update(Property property, UpdateHints? hint = null)
        {
            Update(GenerateUpdateRequest(property, hint));
        }

        public IEnumerable<TransformUpdate> GenerateUpdateRequest(Property property, UpdateHints? hint = null)
        {
            yield return new TransformUpdate(this, property);

            if ((hint == UpdateHints.UpdatedChange || hint == UpdateHints.ToggledEnable) && _boneControllers.Any(x => !x.TransformChildren) ||
                hint == UpdateHints.UpdatedTransformChildren)
            {
                foreach (var child in Children)
                {
                    yield return new TransformUpdate(child, property);
                    if (property.Type == TransformType.Scale)
                        yield return new TransformUpdate(child, TransformType.Position);
                }
            }

            if ((hint == UpdateHints.UpdatedChange || hint == UpdateHints.ToggledEnable) && _boneControllers.Any(x => x.TransformChildren) ||
                hint == UpdateHints.UpdatedTransformChildren)
            {
                foreach (var child in this.GatherMany(x => x.VirtualChildren))
                {
                    yield return new TransformUpdate(child, property);
                    if (property.Type == TransformType.Scale)
                        yield return new TransformUpdate(child, TransformType.Position);
                }
            }
        }

        static void Update(IEnumerable<TransformUpdate> requests)
        {
            foreach(var request in requests.Distinct())
            {
                request.handler.Update(request.type);
            }
        }

        public void Update(TransformType type)
        {
            var value = Vector3.zero;
            foreach (var property in EnumHelper.GetValues<Dimension>().Select(x => new Property(type, x)))
            {
                value = value.Set(property.Dimension, GetValue(property));
            }

            if (!Equals(_transform.Get(type), value))
                _transform.Set(type, value);
        }

        float GetValue(Property property)
        {
            var value = _data.Get(property);

            foreach (var entry in _boneControllers
                .Select(x => x.Properties[property])
                .Where(x => x.IsOverallEnabled))
            {
                value = GetValue(property.Type, value, entry.Change);
            }

            var virtualParents = this.Gather(x => x.VirtualParent);
            var parents = this.Gather(x => x.Parent);
            foreach (var controller in virtualParents.Except(parents).SelectMany(x => x._boneControllers).Where(x => x.TransformChildren))
            {
                var entry = controller.Properties[property];
                if (entry.IsOverallEnabled)
                    value = GetValue(property.Type, value, entry.Change);

                if (property.Type == TransformType.Position)
                {
                    var scaleProperty = new Property(TransformType.Scale, property.Dimension);
                    var scaleEntry = controller.Properties[scaleProperty];
                    if (scaleEntry.IsOverallEnabled)
                    {
                        var offset = GetRelativePosition(controller.Handler._data.position, controller.Handler._data.rotation, _data.position).Get(property.Dimension);
                        var scaledOffset = offset * (scaleEntry.Change - 1);
                        value = GetValue(property.Type, value, scaledOffset);
                    }
                }
            }


            // Counter Changes
            if (Parent != null)
            {
                if (!virtualParents.Contains(Parent))
                {
                    foreach (var controller in Parent._boneControllers)
                    {
                        var entry = controller.Properties[property];
                        if (entry.IsOverallEnabled)
                        {
                            var change = GetCounterChange(property.Type, entry.Change);
                            value = GetValue(property.Type, value, change);
                        }
                        if (property.Type == TransformType.Position)
                        {
                            var scaleProperty = new Property(TransformType.Scale, property.Dimension);
                            var scaleEntry = controller.Properties[scaleProperty];
                            if (scaleEntry.IsOverallEnabled)
                            {
                                var offset = GetRelativePosition(controller.Handler._data.position, controller.Handler._data.rotation, _data.position).Get(property.Dimension);
                                var change = offset * (scaleEntry.Change - 1);
                                change = GetCounterChange(property.Type, change);
                                value = GetValue(property.Type, value, change);
                            }
                        }
                    }
                }
            }

            return value;
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