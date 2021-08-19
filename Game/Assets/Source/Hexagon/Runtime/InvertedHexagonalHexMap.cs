using System;

namespace SomeProject.Hexagon
{
    // We do not need any complex algorithms here, so we store the map simply as an iliffe vector
    // and index the items by their position in the array.
    public class HourGlassHexMap<T>
    {
        public T[][] Grid;
        public int NeckWidth => Grid[Radius].Length;
        public int Diameter => Grid.Length;
        public int Radius => Grid.Length / 2;
        public int Count => Radius * Radius + NeckWidth * Diameter;

        public HourGlassHexMap(int neckWidth, int radius)
        {
            Initialize(neckWidth, radius);
        }

        public void ReInitialize(int neckWidth, int radius)
        {
            if (NeckWidth != neckWidth || radius != Radius)
            {
                Initialize(neckWidth, radius);
            }
        }

        public void Initialize(int neckWidth, int radius)
        {
            int diameter = radius * 2 + 1;
            Grid = new T[diameter][];

            for (int row = 0; row < diameter; row++)
            {
                int rowSize = Math.Abs(radius - row) + neckWidth; 
                Grid[row] = new T[rowSize];
            }
        }
    }

}