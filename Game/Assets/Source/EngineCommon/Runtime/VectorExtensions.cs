using UnityEngine;

namespace EngineCommon
{
    public static class VectorExtensions
    {
        public static Vector2 Rotate(this Vector2 vec, float radians)
        {
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(cos * vec.x - sin * vec.y, sin * vec.x + cos * vec.y);
        }

        public static Vector2 RotateAround(this Vector2 vec, float radians, Vector2 point)
        {
            var offset = vec - point;
            return Rotate(offset, radians) + point;
        }
    }
}