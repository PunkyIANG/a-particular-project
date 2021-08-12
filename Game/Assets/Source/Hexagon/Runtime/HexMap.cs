using System.Collections.Generic;

namespace SomeProject.Hexagon
{
    public interface IHexMap<T> : IEnumerable<HexAxialValuePair<T>>
    {
        int Count { get; }
        T this[HexCube index] { get; set; }
        IEnumerable<T> Values { get; }
        IEnumerable<HexAxial> Coordinates { get; }
    }

    public interface IHexagonalHexMap<T> : IHexMap<T>
    {
        int Diameter { get; }
        int Radius { get; }
        HexAxial Center { get; }
        void ReInitialize(int radius);
        void InstantiateEach(System.Func<HexAxial, T> Instantiator);
    }

    public readonly struct HexAxialValuePair<T>
    {
        public readonly HexAxial Coordinate;
        public readonly T Value;

        public HexAxialValuePair(HexAxial coord, T value)
        {
            Coordinate = coord;
            Value = value;
        }
    }

}