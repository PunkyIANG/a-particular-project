using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Botan.Keg.Editor.Tests
{
    /// <summary>
    /// This is an example script for testing a package's editor script.
    /// </summary>
    public class TestEditorExample
    {
        /// <summary>
        /// A Test behaves as an ordinary method
        /// </summary>
        [Test]
        public void TestEditorExampleSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        /// <summary>
        /// A UnityTest behaves like a coroutine in Play Mode.
        /// In Edit Mode you can use the following line to skip a frame:
        /// <code>yield return null;</code>
        /// </summary>
        [UnityTest]
        public IEnumerator TestEditorExampleWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
