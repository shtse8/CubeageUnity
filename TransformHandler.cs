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
                var current = transform?.parent;
                while (current)
                {
                    if (manager.TryGet(current, out var handler) && handler.IsValid())
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
                var parent = transform.parent;
                for (var i = 0; i < parent.childCount; i++)
                {
                    var child = parent.GetChild(i);
                    if (Equals(child, transform))
                        continue;
                    if (manager.TryGet(child, out var handler) && handler.IsValid())
                        yield return handler;
                }
            }
        }

        public IEnumerable<TransformHandler> Children => GetChildren(transform);

        [SerializeField]
        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (Equals(isExpanded, value))
                    return;

                isExpanded = value;
            }
        }

        [SerializeField]
        [SerializeReference]
        protected Transform transform;
        public Transform Transform
        {
            get => transform;
            set
            {
                if (Equals(transform, value))
                    return;

                transform = value;
                data = new TransformData
                {
                    localPosition = value.localPosition,
                    localEulerAngles = value.localEulerAngles,
                    localScale = value.localScale,
                    // position = GetRelativePosition(_manager.Root.transform, value.position),
                    position = value.position,
                    rotation = value.rotation,
                };
            }
        }

        [SerializeField]
        protected TransformData data;
        public TransformData Data => data;

        [SerializeField]
        [SerializeReference]
        protected List<TransformController> boneControllers = new List<TransformController>();
        public List<TransformController> BoneControllers => boneControllers.ToList();

        public string Name => transform?.name;

        #region Virtual Hierarchy
        [SerializeField]
        [SerializeReference]
        protected TransformHandler virtualParent;
        public TransformHandler VirtualParent
        {
            get => virtualParent != null && virtualParent.IsValid() ? virtualParent : null;
            set
            {
                if (Equals(virtualParent, value))
                    return;

                if (Equals(this, value))
                    throw new Exception("Cannot set itself as parent.");

                var parent = value;
                while (parent != null)
                {
                    if (Equals(parent, this))
                        throw new Exception("Cannot set itself due to recursion.");
                    parent = parent.VirtualParent;
                }

                virtualParent = value;
                Update();
            }
        }

        public IEnumerable<TransformHandler> VirtualChildren => manager.Handlers.Where(x => Equals(x.VirtualParent, this));
        #endregion

        [SerializeField]
        [SerializeReference]
        protected TransformManager manager;

        private IEnumerable<TransformHandler> GetChildren(Transform transform)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (manager.TryGet(child, out var handler) && handler.IsValid())
                    yield return handler;
                else
                    foreach(var x in GetChildren(child))
                        yield return x;
            }
        }

        public TransformHandler(TransformManager manager, Transform transform)
        {
            this.manager = manager;
            Transform = transform;
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
            boneControllers.Remove(controller);
            Update(controller, UpdateHints.ToggledEnable);
        }

        public void AddTransformController(TransformController controller)
        {
            if (boneControllers.Contains(controller))
                throw new Exception("Duplicated bone controller.");
            boneControllers.Add(controller);
            Update(controller, UpdateHints.ToggledEnable);
        }

        public void SetVirtualParent(Transform transform)
        {
            if (!manager.TryGet(transform, out var handler))
                throw new Exception("Not a valid parent.");
            VirtualParent = handler;
        }

        public void AddVirtualChild(Transform transform)
        {
            if (!manager.TryGet(transform, out var handler))
                throw new Exception("Not a valid child.");
            Debug.Log(handler.VirtualParent.Transform.name);
            handler.VirtualParent = this;
            Debug.Log(handler.VirtualParent.Transform.name);
        }

        public bool IsValid()
        {
            return transform;
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
            var targetProperties = boneControllers.Intersect(this.boneControllers)
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

            if ((hint == UpdateHints.UpdatedChange || hint == UpdateHints.ToggledEnable) && boneControllers.Any(x => !x.TransformChildren) ||
                hint == UpdateHints.UpdatedTransformChildren)
            {
                foreach (var child in Children)
                {
                    yield return new TransformUpdate(child, property);
                    if (property.Type == TransformType.Scale || property.Type == TransformType.Rotation)
                        yield return new TransformUpdate(child, TransformType.Position);
                }
            }

            if ((hint == UpdateHints.UpdatedChange || hint == UpdateHints.ToggledEnable) && boneControllers.Any(x => x.TransformChildren) ||
                hint == UpdateHints.UpdatedTransformChildren)
            {
                foreach (var child in this.GatherMany(x => x.VirtualChildren))
                {
                    yield return new TransformUpdate(child, property);
                    if (property.Type == TransformType.Scale || property.Type == TransformType.Rotation)
                        yield return new TransformUpdate(child, TransformType.Position);
                }
            }
        }

        private static void Update(IEnumerable<TransformUpdate> requests)
        {
            foreach(var request in requests.Distinct())
            {
                request.Handler.Update(request.Type);
            }
        }

        public void Update(TransformType type)
        {
            if (!IsValid())
                return;

            Vector3 value = GetValue(type);
            if (!Equals(transform.Get(type), value))
                transform.Set(type, value);
        }

        private Vector3 GetChange(TransformController controller, TransformType type)
        {
            var change = type == TransformType.Scale ? Vector3.one : Vector3.zero;
            if (controller.Properties[new Property(type, Dimension.X)].IsOverallEnabled)
                change.x = controller.Properties[new Property(type, Dimension.X)].Change;
            if (controller.Properties[new Property(type, Dimension.Y)].IsOverallEnabled)
                change.y = controller.Properties[new Property(type, Dimension.Y)].Change;
            if (controller.Properties[new Property(type, Dimension.Z)].IsOverallEnabled)
                change.z = controller.Properties[new Property(type, Dimension.Z)].Change;
            return change;
        }

        private Vector3 GetValue(TransformType type)
        {
            var value = data.Get(type);
            foreach (var controller in boneControllers)
            {
                var change = GetChange(controller, type);
                value = Change(type, value, change);
            }

            var virtualParents = this.Gather(x => x.VirtualParent);
            var parents = this.Gather(x => x.Parent);
            foreach (var controller in virtualParents.Except(parents).SelectMany(x => x.boneControllers).Where(x => x.TransformChildren))
            {
                var change = GetChange(controller, type);
                value = Change(type, value, change);

                if (type == TransformType.Position)
                {
                    var relativePosition = GetRelativePosition(controller.Handler.data.position, controller.Handler.data.rotation, data.position);
                    var scaleChange = GetChange(controller, TransformType.Scale);

                    // Handle Scale
                    var scaleOffset = Vector3.Scale(relativePosition, scaleChange - Vector3.one);
                    value = Change(type, value, scaleOffset);

                    // Handle Rotation
                    var rotationChange = GetChange(controller, TransformType.Rotation);
                    if (!Equals(rotationChange, Vector3.zero))
                    {
                        var rotationOffset = Rotate(relativePosition, rotationChange) - relativePosition;
                        rotationOffset = Vector3.Scale(rotationOffset, scaleChange);
                        value = Change(type, value, rotationOffset);
                    }
                }
            }

            // Counter Changes
            if (Parent != null)
            {
                if (!virtualParents.Where(x => x.BoneControllers.Any(y => y.TransformChildren)).Contains(Parent))
                {
                    foreach (var controller in Parent.boneControllers)
                    {
                        var change = GetChange(controller, type);
                        value = CounterChange(type, value, change);

                        if (type == TransformType.Position)
                        {
                            var relativePosition = GetRelativePosition(controller.Handler.data.position, controller.Handler.data.rotation, data.position);
                            var scaleChange = GetChange(controller, TransformType.Scale);

                            // Handle Scale
                            var scaleOffset = Vector3.Scale(relativePosition, scaleChange.Invert() - Vector3.one);
                            value = Change(type, value, scaleOffset);

                            // Handle Rotation
                            var rotationChange = -GetChange(controller, TransformType.Rotation);
                            if (!Equals(rotationChange, Vector3.zero))
                            {
                                var rotationOffset = Rotate(relativePosition, rotationChange) - relativePosition;
                                rotationOffset = Vector3.Scale(rotationOffset, scaleChange.Invert());
                                value = Change(type, value, rotationOffset);
                            }

                            // Handle Rotation
                            // var rotationChange = GetChange(controller, TransformType.Rotation);
                            // if (!Equals(rotationChange, Vector3.zero))
                            // {
                            //     var offset = (Rotate(relativePosition, Quaternion.Inverse(Quaternion.Euler(rotationChange)).eulerAngles) - relativePosition).Get(property.Dimension);
                            //     if (scaleEntry.IsOverallEnabled)
                            //         offset *= scaleEntry.Change;
                            //     value = GetValue(property.Type, value, offset);
                            // }
                        }
                    }
                }
            }

            return value;
        }

        private static Vector3 Rotate(Vector3 vector, Vector3 angles)
        {
            vector = RotateX(vector, angles.x);
            vector = RotateY(vector, angles.y);
            vector = RotateZ(vector, angles.z);
            return vector;
        }

        public static Vector3 RotateX(Vector3 vector, float angle)
        {
            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);


            var result = vector;
            result.y = (cos * vector.y) - (sin * vector.z);
            result.z = (cos * vector.z) + (sin * vector.y);

            return result;
        }

        public static Vector3 RotateY(Vector3 vector, float angle)
        {
            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

            var result = vector;
            result.x = (cos * vector.x) + (sin * vector.z);
            result.z = (cos * vector.z) - (sin * vector.x);
            return result;
        }

        public static Vector3 RotateZ(Vector3 vector, float angle)
        {
            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

            var result = vector;
            result.x = (cos * vector.x) - (sin * vector.y);
            result.y = (cos * vector.y) + (sin * vector.x);
            return result;
        }

        private float GetCounterChange(TransformType type, float change)
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

        private Vector3 Change(TransformType type, Vector3 value, Vector3 change)
        {
            switch (type)
            {
                case TransformType.Position:
                    return value + change;
                case TransformType.Rotation:
                    return (Quaternion.Euler(value) * Quaternion.Euler(change)).eulerAngles;
                case TransformType.Scale:
                    return Vector3.Scale(value, change);
                default:
                    throw new Exception("Unknown Type.");
            }
        }

        private Vector3 CounterChange(TransformType type, Vector3 value, Vector3 change)
        {
            switch (type)
            {
                case TransformType.Position:
                    return value - change;
                case TransformType.Rotation:
                    // Still don't understand why, but it works.
                    var rotation = Quaternion.Euler(change);
                    return Quaternion.Inverse(Quaternion.Inverse(Quaternion.Euler(value)) * rotation).eulerAngles;
                case TransformType.Scale:
                    return Vector3.Scale(value, change.Invert());
                default:
                    throw new Exception("Unknown Type.");
            }
        }

        public bool TryGetTargetTransform(out Transform target)
        {
            target = null;
            if (Equals(transform.parent.name + manager.Suffix, transform.name))
            {
                target = transform.parent;
                return true;
            }
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (Equals(child.name + manager.Suffix, transform.name))
                {
                    target = child;
                    return true;
                }
            }
            return false;
        }

    }

}