using UnityEngine;

namespace SomeProject.Hexagon
{
    public static class HexVectorTransformations
    {
        public static readonly float WIDTH_HEIGTH_RATIO = Mathf.Sqrt(3) / 2; 
        public static readonly float sqrt3 = Mathf.Sqrt(3); 
        public static readonly float sqrt3_2 = Mathf.Sqrt(3) / 2; 
        public static readonly float sqrt3_3 = Mathf.Sqrt(3) / 3; 

        public static Vector2 ToWorldCoordinate(this HexAxial axial, float height)
        {
            float x = height * (sqrt3 * axial.q + sqrt3_2 * axial.r);
            float y = height * 3f / 2 * axial.r;
            return new Vector2(x, y);
        }

        public static HexAxial ToAxialCoordinate(this Vector2 world, float height)
        {
            var q = (sqrt3_3 * world.x - 1f/3 * world.y) / height;
            var r = 2f / 3 * world.y / height;
            return new HexAxial(Mathf.RoundToInt(r), Mathf.RoundToInt(q));
        }
    }
}