using System;
using System.Collections.Generic;
using UnityEngine;

namespace SomeProject.Hexagon
{
    /// Maps coordinates to game objects
    public class HexagonalWraparoundMap<T>
    {
        public T[][] _grid;
        private HexCube[] _mirroredCenters;

        public int Diameter => _grid.Length;
        public int Radius => Diameter / 2; 

        private static readonly Dictionary<int, HexCube[]> _MirroredMapCenters = new Dictionary<int, HexCube[]>();

        // https://gamedev.stackexchange.com/a/137603
        public static void ReinitializeGlobalsForMapSize(int diameter)
        {
            if (_MirroredMapCenters.ContainsKey(diameter))
                return;
            
            // The origin + 6 neighboring mirror grids
            var array = new HexCube[7];
            // The origin is 0, 0, 0
            array[0] = new HexCube();
            array[1] = new HexCube(2 * diameter + 1, -diameter);

            for (int i = 2; i < 7; i++)
            {
                array[i] = array[i - 1].RotateOneSixthClokwise(); 
            }

            _MirroredMapCenters.Add(diameter, array);
        }

        public HexagonalWraparoundMap(int diameter, System.Func<HexAxial, T> instantiator)
        {
            Initialize(diameter, instantiator);
        }

        public void Initialize(int diameter, System.Func<HexAxial, T> instantiator)
        {
            Debug.Assert((diameter & 1) == 1);
            Debug.Assert(_MirroredMapCenters.ContainsKey(diameter));

            _mirroredCenters = _MirroredMapCenters[diameter];
            _grid = new T[diameter][];

            for (int row = 0; row < diameter; row++)
            {
                int rowSize = 2 * diameter + 1 - Math.Abs(diameter - row);
                var rowArray = new T[rowSize];
                _grid[row] = rowArray;
                
                for (int j = 0; j < diameter; j++)
                {
                    rowArray[j] = instantiator(IndicesToAxial(row, j));
                }
            }
        }

        public HexAxial IndicesToAxial(int r, int q)
        {
            return new HexAxial(r, UngetQIndex(r, q)); 
        }

        public HexCube WrapAround(HexCube cube)
        {
            for (int i = 0; i < 7; i++)
            {
                if (cube.GetDistanceTo(_mirroredCenters[i]) < Radius)
                {
                    return cube - _mirroredCenters[i];
                }
            }
            // We're way past the map, which should never happen!
            Debug.Assert(false);
            return cube;
        }

        public int UngetQIndex(int r, int q)
        {
            return q + Math.Max(0, _grid.Length - r);
        }

        public int GetQIndex(HexAxial axial)
        {
            return axial.q - Math.Max(0, _grid.Length - axial.r);
        }

        public T GetForWrapped(HexCube wrappedAroundCoord)
        {
            int qIndex = GetQIndex(wrappedAroundCoord);
            return _grid[wrappedAroundCoord.r][qIndex];
        }

        public void SetForWrapped(HexCube wrappedAroundCoord, T value)
        {
            int qIndex = GetQIndex(wrappedAroundCoord);
            _grid[wrappedAroundCoord.r][qIndex] = value;
        }

        public T this[HexCube cube]
        {
            get => GetForWrapped(WrapAround(cube));
            set => SetForWrapped(WrapAround(cube), value);
        }
    }
}