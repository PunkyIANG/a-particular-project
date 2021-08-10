using UnityEngine;
using NUnit.Framework;

namespace EngineCommon.Tests
{
    public class VectorExtensionsTests
    {
        private float Epsilon = 0.01f;
        [Test]
        public void Rotation_IsCorrect()
        {
            var vec = new Vector2(2, 2); 
            var rotated = vec.Rotate(Mathf.PI / 2);
            Assert.AreEqual(-2, rotated.x, Epsilon);
            Assert.AreEqual(2, rotated.y, Epsilon);
        }
        
        [Test]
        public void RotationAroundPole_IsCorrect()
        {
            var vec = new Vector2(2, 1); 
            var around = new Vector2(0, 1);
            var rotated = vec.RotateAround(Mathf.PI / 2, around);
            Assert.AreEqual(0, rotated.x, Epsilon);
            Assert.AreEqual(3, rotated.y, Epsilon);
        }
    }
}