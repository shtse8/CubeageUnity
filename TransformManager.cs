using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


namespace Cubeage
{
    [Serializable]
    public class TransformManager
    {
        [SerializeField]
        protected string suffix;
        public string Suffix => suffix;

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
        
        [SerializeReference]
        [SerializeField]
        protected Transform root;
        public Transform Root {
            get => root;
            set
            {
                if (Equals(root, value))
                    return;

                root = value;
                Reload();
            }
        }

        [SerializeReference]
        [SerializeField]
        protected List<TransformHandler> handlers = new List<TransformHandler>();
        public List<TransformHandler> Handlers => handlers.ToList();

        public TransformManager(string suffix = "_ctrl")
        {
            this.suffix = suffix;
        }

        public bool IsValid(Transform transform)
        {
            return transform.name.EndsWith(suffix);
        }

        public TransformHandler Get(Transform transform)
        {
            if (!IsValid(transform))
                throw new Exception("Component is not valid.");

            if (!TryGet(transform, out var handler))
                throw new Exception("This part doesn't belong to this avatar.");

            return handler;
        }

        public bool TryGet(Transform transform, out TransformHandler handler)
        {
            handler = handlers.FirstOrDefault(x => Equals(transform, x.Transform));
            return handler != null;
        }


        public void Reload()
        {
            var transforms = root.GetComponentsInChildren<Transform>().Where(x => IsValid(x));

            // Remove invalid handlers
            foreach (var handler in handlers.ToArray())
            {
                if (!handler.IsValid() || !transforms.Contains(handler.Transform))
                {
                    handlers.Remove(handler);
                }
            }

            // Add new handlers
            var newHandlers = new List<TransformHandler>();
            foreach (var transform in transforms)
            {
                if (!TryGet(transform, out _))
                {
                    var handler = new TransformHandler(this, transform);
                    handlers.Add(handler);
                    newHandlers.Add(handler);
                }
            }

            foreach (var handler in newHandlers)
            {
                AutoSetParent(handler);
            }
        }

        public void Update()
        {
            foreach (var handler in handlers)
            {
                handler.Update();
            }
        }

        public void Update(IEnumerable<TransformController> boneControllers, UpdateHints? hint = null)
        {
            if (hint == UpdateHints.UpdatedChange)
                boneControllers = boneControllers.Where(x => x.IsEnabled);
            foreach (var handler in handlers.Where(x => x.BoneControllers.Intersect(boneControllers).Any()))
            {
                handler.Update(boneControllers, hint);
            }
        }

        public void AutoSetParent()
        { 
            foreach (var handler in handlers)
            {
                AutoSetParent(handler);
            }
        }

        public void AutoSetParent(TransformHandler handler)
        {
            var parent = handler.TryGetTargetTransform(out var target) ? target.parent : handler.Transform.parent;
            if (parent != null && TryGetHandler(parent, out var targetHandler))
                handler.VirtualParent = targetHandler;
            else
                handler.VirtualParent = handler.Parent;
        }
        public bool TryGetHandler(Transform transform, out TransformHandler handler)
        {
            handler = handlers.FirstOrDefault(x => x.TryGetTargetTransform(out var target) && target == transform);
            return handler != null;
        }
    }

}