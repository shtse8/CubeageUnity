using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cubeage
{
    [Serializable]
    public struct Property
    {
        [SerializeField]
        private TransformType _type;
        public TransformType Type => _type;

        [SerializeField]
        private Dimension _dimension;
        public Dimension Dimension => _dimension;

        public Property(TransformType type, Dimension dimension)
        {
            _type = type;
            _dimension = dimension;
        }

        public static explicit operator Property(Tuple<TransformType, Dimension> tuple)
        {
            return new Property(tuple.Item1, tuple.Item2);
        }

        public override string ToString() => $"{_type}{_dimension}";

        public static IEnumerable<Property> GetAll()
        {
            var properties = new List<Property>();
            foreach (var type in EnumHelper.GetValues<TransformType>())
            {
                foreach (var direction in EnumHelper.GetValues<Dimension>())
                {
                    properties.Add(new Property(type, direction));
                }
            }
            return properties;
        }
    }

}