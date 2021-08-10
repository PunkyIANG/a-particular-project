using EngineCommon;
using NUnit.Framework;
using UnityEngine;

namespace SomeProject.Hexagon.Tests
{
    public class CoordinateSystemsTests
    {
        [Test]
        public void AxialWorldConvertion_IsReversible()
        {
            var axial = new HexAxial(1, 5);
            var world = axial.ToWorldCoordinate(1.5f);
            var axialAfter = world.ToAxialCoordinate(1.5f);
            Assert.AreEqual(axial, axialAfter);
        }

        [Test]
        public void Rotation_IsCorrect()
        {
            var cube = new HexCube(5, 3);
            var world = cube.Axial.ToWorldCoordinate(2.5f);
            var cubeRotated = cube.RotateOneSixthClokwise();
            var worldRotated = cubeRotated.Axial.ToWorldCoordinate(2.5f);
            var angle = Vector2.Angle(world, worldRotated);
            Assert.AreEqual(60, angle, 0.5f);
        }

        [Test]
        public void Rotation_IsReversible()
        {
            var cube = new HexCube(4, 2);
            var cubeRotated = cube.RotateOneSixthClokwise();
            var cubeRotatedBack = cubeRotated.RotateOneSixthCounterClokwise();
            Assert.AreEqual(cube, cubeRotatedBack);
        }

        [Test]
        public void Distance_IsCorrect()
        {
            var cube = new HexCube(1, 2, -3);
            var otherCube = new HexCube(1, 0, -1);
            Assert.AreEqual(2, cube.GetDistanceTo(otherCube));
        }

        [Test]
        public void RotationAroundPole_IsCorrect()
        {
            var cube = new HexCube(3, 2);
            var pole = new HexCube(1, -1);
            var cubeRotated = cube.RotateOneSixthClokwise(pole);

            var world = cube.Axial.ToWorldCoordinate(1.5f);
            var poleWorld = pole.Axial.ToWorldCoordinate(1.5f);
            var worldRotated = world.RotateAround(Mathf.PI / 3, poleWorld);
            var cubeRotatedWithExtraSteps = worldRotated.ToAxialCoordinate(1.5f);

            Assert.AreEqual(cubeRotatedWithExtraSteps, cubeRotated.Axial);
        }
    }
}