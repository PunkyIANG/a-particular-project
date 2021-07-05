using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PlayTests
{

    public class ExamplePlayTest
    {
        [UnityTest]
        public IEnumerator ExampleTest()
        {
            Time.timeScale = 20f;

            GameObject gameObject = new GameObject();
            gameObject.transform.position = Vector3.zero;
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;

            float time = 0;

            while (time < 5)
            {
                time += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(gameObject.transform.position.y < -1f);
            
            Time.timeScale = 1f;
        }
    }

}