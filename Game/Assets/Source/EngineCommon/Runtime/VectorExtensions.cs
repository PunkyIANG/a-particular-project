using UnityEngine;

namespace EngineCommon
{
    public static class VectorExtensions
    {
        public static Vector2 Rotate(this Vector2 vec, float radians)
        {
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * vec.magnitude;
        }

        public static Vector2 RotateAround(this Vector2 vec, float radians, Vector2 point)
        {
            var offset = vec - point;
            return Rotate(offset, radians) + point;
        }
    }
}