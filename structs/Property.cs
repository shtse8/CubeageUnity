using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cubeage
{
    [Serializable]
    public struct Property
    {
        [SerializeField]
        private TransformType type;
        public TransformType Type => type;

        [SerializeField]
        private Dimension dimension;
        public Dimension Dimension => dimension;

        public Property(TransformType type, Dimension dimension)
        {
            this.type = type;
            this.dimension = dimension;
        }

        public static explicit operator Property(Tuple<TransformType, Dimension> tuple)
        {
            return new Property(tuple.Item1, tuple.Item2);
        }

        public override string ToString() => $"{type}{dimension}";

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