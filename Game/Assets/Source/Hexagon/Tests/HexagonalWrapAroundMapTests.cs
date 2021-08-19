using NUnit.Framework;

namespace SomeProject.Hexagon.Tests
{
    public class HexagonalWrapAroundMapTests
    {
        private HexagonalWrapAroundMap<bool> _map;

        [SetUp]
        public void Setup()
        {
            var radius = 1;
            _map = new HexagonalWrapAroundMap<bool>(radius);
            _map.InstantiateEach(a => true);
        }

        private void AssertInBounds(int r, int q)
        {
            Assert.True(_map.IsInBounds(new HexAxial(r, q)));
        }

        
        [Test]
        public void CreationGivesCorrectCoordinates()
        {
            Assert.AreEqual(_map.Count, 7);
            
            // The center
            AssertInBounds(1, 1);
            // Left and right 
            AssertInBounds(1, 0);
            AssertInBounds(1, 2);
            // The top two
            AssertInBounds(0, 1);
            AssertInBounds(0, 2);
            // The bottom two
            AssertInBounds(2, 0);
            AssertInBounds(2, 1);
        }

        [Test]
        public void WrappingWorks()
        {
            // It should just not error out.

            foreach (var axial in _map.Coordinates)
            {
                for (int r = -_map.Radius; r <= _map.Radius; r++)
                {
                    // Figuring out the exact possible offset of q
                    // is as tricky as the algorithm itself.
                    // This seems good enough, even though it does not cover all hexes.
                    for (int q = -_map.Radius; q <= _map.Radius; q++)
                    {
                        var newAxial = axial + new HexAxial(r, q);
                        var wrapped = _map.WrapAround(newAxial);
                        Assert.True(_map.IsInBounds(wrapped));
                    }
                }
            }
        }
    }
}