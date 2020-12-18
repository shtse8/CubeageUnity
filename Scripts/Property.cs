using System;


namespace Cubeage
{
    [Serializable]
    public struct Property
    {
        public TransformType Type;
        public Direction Direction;

        public Property(TransformType type, Direction direction)
        {
            Type = type;
            Direction = direction;
        }

        public static explicit operator Property(Tuple<TransformType, Direction> tuple)
        {
            return new Property(tuple.Item1, tuple.Item2);
        }

        public override string ToString() => $"{Type}{Direction}";
    }

}