using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Botan.Keg.Tests
{
    /// <summary>
    /// This is an example script for testing a package's runtime script.
    /// </summary>
    public class TestRuntimeExample
    {
        /// <summary>
        /// A Test behaves as an ordinary method
        /// </summary>
        [Test]
        public void TestRuntimeExampleSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        /// <summary>
        /// A UnityTest behaves like a coroutine in Play Mode.
        /// In Edit Mode you can use the following line to skip a frame:
        /// <code>yield return null;</code>
        /// </summary>
        [UnityTest]
        public IEnumerator TestRuntimeExampleEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
