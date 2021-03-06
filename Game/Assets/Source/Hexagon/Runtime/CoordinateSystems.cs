using System;
using System.Diagnostics;

namespace SomeProject.Hexagon
{
    public readonly struct HexAxial
    {
        public readonly int r;
        public readonly int q;
        public int S => -q - r;

        public HexAxial(int r, int q)
        {
            this.r = r;
            this.q = q;
        }

        public static implicit operator HexCube(HexAxial axial) => new HexCube(axial.r, axial.q);

        public HexCube Cube => this;

        public int this[int index]
        {
            get
            { 
                switch (index)
                {
                    case 0: return r;
                    case 1: return q;
                    case 2: return S;
                    default: Debug.Assert(false); return 0;
                }
            }
        }

        public override int GetHashCode()
        {
            unchecked
			{
				int hash = 17;
				hash = hash * 23 + q.GetHashCode();
				hash = hash * 23 + r.GetHashCode();
				return hash;
			}
        }

        public override bool Equals(object obj)
        {
            return obj is HexAxial axial &&
                   r == axial.r &&
                   q == axial.q;
        }

        public override string ToString()
        {
            return $"axial<{r}, {q}>";
        }

        public static HexAxial operator-(HexAxial a, HexAxial b)
        {
            return new HexAxial(a.r - b.r, a.q - b.q);
        }
        public static HexAxial operator+(HexAxial a, HexAxial b)
        {
            return new HexAxial(a.r + b.r, a.q + b.q);
        }
    }

    public readonly struct HexCube
    {
        public readonly int r;
        public readonly int q;
        public readonly int s;

        public HexCube(int r, int q)
        {
            this.r = r;
            this.q = q;
            this.s = -q - r;
        }

        public HexCube(int r, int q, int s)
        {
            Debug.Assert(s == -q - r);
            this.r = r;
            this.q = q;
            this.s = s;
        }

        public static implicit operator HexAxial(HexCube cube) => new HexAxial(cube.r, cube.q);

        public HexAxial Axial => this;

        public int this[int index]
        {
            get
            { 
                switch (index)
                {
                    case 0: return r;
                    case 1: return q;
                    case 2: return s;
                    default: Debug.Assert(false); return 0;
                }
            }
        }

        public static HexCube operator-(HexCube a, HexCube b)
        {
            return new HexCube(a.r - b.r, a.q - b.q, a.s - b.s);
        }
        public static HexCube operator+(HexCube a, HexCube b)
        {
            return new HexCube(a.r + b.r, a.q + b.q, a.s + b.s);
        }

        public override bool Equals(object obj)
        {
            return obj is HexCube cube &&
                   r == cube.r &&
                   q == cube.q;
        }

        public override int GetHashCode()
        {
            return Axial.GetHashCode(); 
        }

        public override string ToString()
        {
            return $"cube<{r}, {q}, {s}>";
        }
    }

    // TODO: this is the only part that depends on UnityEngine, so this or the coordinates should be in a separate assembly.
    public static class HexAlgorithms
    {
        public static HexCube RotateOneSixthCounterClokwise(this HexCube cube)
        {
            return new HexCube(-cube.q, -cube.s, -cube.r);
        }

        public static HexCube RotateOneSixthClokwise(this HexCube cube)
        {
            return new HexCube(-cube.s, -cube.r, -cube.q);
        }

        public static HexCube RotateOneSixthCounterClokwise(this HexCube cube, HexCube around)
        {
            var diff = cube - around;
            var rotatedDiff = RotateOneSixthCounterClokwise(diff);
            return around + rotatedDiff;
        }

        public static HexCube RotateOneSixthClokwise(this HexCube cube, HexCube around)
        {
            var diff = cube - around;
            var rotatedDiff = RotateOneSixthClokwise(diff);
            return around + rotatedDiff;
        }

        public static int GetDistanceTo(this HexCube cube, HexCube other)
        {
            int a = Math.Abs(cube.r - other.r);
            int b = Math.Abs(cube.q - other.q);
            int c = Math.Abs(cube.s - other.s);
            return Math.Max(a, Math.Max(b, c));
        }
    }
}