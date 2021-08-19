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
        public static HexCube[] ReinitializeForMapSize(int radius)
        {
            int diameter = radius * 2 + 1;
            HexCube[] array;

            if (_MirroredMapCenters.TryGetValue(diameter, out array))
                return array;
            
            // The origin + 6 neighboring mirror grids
            array = new HexCube[7];
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

            return array;
        }
    }

    /// Maps coordinates to game objects
    public class HexagonalWrapAroundMap<T> : IHexagonalHexMap<T>
    {
        public T[][] _grid;
        private HexCube[] _mirroredCenters;

        public int Diameter => _grid.Length;
        public int Radius => Diameter / 2; 
        public int Count => 3 * Radius * (Radius + 1) + 1;
        public HexAxial Center => new HexAxial(Radius, Radius);
        public bool IsInitialized => !(_grid is null);
        
        public HexagonalWrapAroundMap()
        {
        }

        public HexagonalWrapAroundMap(int radius)
        {
            Initialize(radius);
        }

        public void ReInitialize(int radius)
        {
            if (radius != Radius)
            {
                Initialize(radius);
            }
        }

        public void Initialize(int radius)
        {
            int diameter = radius * 2 + 1;
            
            // Let's just forget about threads?
            _mirroredCenters = HexagonalWrapAroundMapSharedGlobals.ReinitializeForMapSize(radius);
            _grid = new T[diameter][];

            for (int row = 0; row < diameter; row++)
            {
                int rowSize = diameter - Math.Abs(radius - row);
                _grid[row] = new T[rowSize];
            }
        }
        
        public void InstantiateEach(System.Func<HexAxial, T> Instantiator)
        {
            for (int row = 0; row < _grid.Length; row++)
            {
                var rowArray = _grid[row];
                for (int j = 0; j < rowArray.Length; j++)
                {
                    rowArray[j] = Instantiator(IndicesToAxial(row, j));
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
        public IEnumerator<HexAxialValuePair<T>> GetEnumerator()
        {
            for (int row = 0; row < _grid.Length; row++)
            for (int j = 0; j < _grid[row].Length; j++)
            {
                yield return new HexAxialValuePair<T>(IndicesToAxial(row, j), _grid[row][j]);
            }
        }

        public IEnumerable<HexAxial> Coordinates
        {
            get
            {
                for (int row = 0; row < _grid.Length; row++)
                for (int j = 0; j < _grid[row].Length; j++)
                {
                    yield return IndicesToAxial(row, j);
                }
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                for (int row = 0; row < _grid.Length; row++)
                for (int j = 0; j < _grid[row].Length; j++)
                {
                    yield return _grid[row][j];
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