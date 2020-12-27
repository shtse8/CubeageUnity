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
        protected string _suffix;
        public string Suffix => _suffix;

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
        protected Transform _root;
        public Transform Root {
            get => _root;
            set
            {
                if (Equals(_root, value))
                    return;

                _root = value;
                _handlers.Clear();
                _handlers.AddRange(_root.GetComponentsInChildren<Transform>().Where(x => IsValid(x)).Select(x => new TransformHandler(this, x)));
                foreach(var handler in _handlers)
                    handler.VirtualParent = handler.Parent;
            }
        }

        [SerializeReference]
        [SerializeField]
        protected List<TransformHandler> _handlers = new List<TransformHandler>();
        public List<TransformHandler> Handlers => _handlers.ToList();

        public TransformManager(string suffix = "_ctrl")
        {
            _suffix = suffix;
        }

        public bool IsValid(Transform transform)
        {
            return transform.name.EndsWith(_suffix);
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
            handler = _handlers.FirstOrDefault(x => Equals(transform, x.Transform));
            return handler != null;
        }


        public void Update()
        {
            foreach (var handler in _handlers)
            {
                handler.Update();
            }
        }

        public void Update(IEnumerable<TransformController> boneControllers, UpdateHints? hint = null)
        {
            if (hint == UpdateHints.UpdatedChange)
                boneControllers = boneControllers.Where(x => x.IsEnabled);
            foreach (var handler in _handlers.Where(x => x.BoneControllers.Intersect(boneControllers).Any()))
            {
                handler.Update(boneControllers, hint);
            }
        }

    }

}