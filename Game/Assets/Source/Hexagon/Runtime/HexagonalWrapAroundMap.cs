using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SomeProject.Hexagon
{
    public static class HexagonalWrapAroundMapSharedGlobals
    {
        public static readonly Dictionary<int, HexCube[]> _MirroredMapCenters = new Dictionary<int, HexCube[]>();

        // https://gamedev.stackexchange.com/a/137603
        public static void ReinitializeForMapSize(int radius)
        {
            int diameter = radius * 2 + 1;

            if (_MirroredMapCenters.ContainsKey(diameter))
                return;
            
            // The origin + 6 neighboring mirror grids
            var array = new HexCube[7];
            array[0] = new HexCube(radius, radius);
            array[1] = new HexCube(diameter, -radius);

            for (int i = 2; i < 7; i++)
            {
                array[i] = array[i - 1].RotateOneSixthClokwise(); 
            }

            for (int i = 1; i < 7; i++)
            {
                array[i] += array[0];
            }

            _MirroredMapCenters.Add(diameter, array);
        }
    }

    /// Maps coordinates to game objects
    public class HexagonalWrapAroundMap<T> : IEnumerable<T>
    {
        public T[][] _grid;
        private HexCube[] _mirroredCenters;

        public int Diameter => _grid.Length;
        public int Radius => Diameter / 2; 
        public int Count => 3 * Radius * (Radius + 1) + 1;
        public HexAxial Center => new HexAxial(Radius, Radius);
        

        public HexagonalWrapAroundMap(int radius, System.Func<HexAxial, T> instantiator = null)
        {
            Initialize(radius, instantiator);
        }

        public void Initialize(int radius, System.Func<HexAxial, T> instantiator = null)
        {
            int diameter = radius * 2 + 1;

            Debug.Assert(HexagonalWrapAroundMapSharedGlobals._MirroredMapCenters.ContainsKey(diameter));

            _mirroredCenters = HexagonalWrapAroundMapSharedGlobals._MirroredMapCenters[diameter];
            _grid = new T[diameter][];

            for (int row = 0; row < diameter; row++)
            {
                int rowSize = diameter - Math.Abs(radius - row);
                var rowArray = new T[rowSize];
                _grid[row] = rowArray;
                
                if (instantiator is null)
                {
                    continue;
                }

                for (int j = 0; j < rowSize; j++)
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
                if (cube.GetDistanceTo(_mirroredCenters[i]) <= Radius)
                {
                    return cube - _mirroredCenters[i] + Center.Cube;
                }
            }
            // We're way past the map, which should never happen!
            Debug.Assert(false, cube);
            return cube;
        }

        public int UngetQIndex(int r, int q)
        {
            return q + Math.Max(0, Radius - r);
        }

        public int GetQIndex(HexAxial axial)
        {
            return axial.q - Math.Max(0, Radius - axial.r);
        }

        public T GetForWrapped(HexAxial wrappedAroundCoord)
        {
            int qIndex = GetQIndex(wrappedAroundCoord);
            return _grid[wrappedAroundCoord.r][qIndex];
        }

        public void SetForWrapped(HexAxial wrappedAroundCoord, T value)
        {
            int qIndex = GetQIndex(wrappedAroundCoord);
            _grid[wrappedAroundCoord.r][qIndex] = value;
        }

        public bool IsInBounds(HexAxial axial)
        {
            return _grid.Length > axial.r 
                && _grid[axial.r].Length > GetQIndex(axial);
        }

        // Left to right, top to bottom
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _grid.Length; i++)
            {
                for (int j = 0; j < _grid[i].Length; j++)
                {
                    yield return _grid[i][j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T this[HexCube cube]
        {
            get => GetForWrapped(WrapAround(cube));
            set => SetForWrapped(WrapAround(cube), value);
        }
    }
}