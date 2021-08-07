using UnityEngine;

namespace SomeProject.Hexagon
{
    public class Test : MonoBehaviour
    {
        private static readonly float sqrt3_2 = Mathf.Sqrt(3) / 2;
        private Mesh MakeHex()
        {
            var mesh = new Mesh();
            
            mesh.vertices = new Vector3[] 
            { 
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0.5f, 0, sqrt3_2),
                new Vector3(-0.5f, 0, sqrt3_2),
                new Vector3(-1, 0, 0),
                new Vector3(-0.5f, 0, -sqrt3_2),
                new Vector3(0.5f, 0, -sqrt3_2)
            };
            mesh.triangles = new int[]
            {
                0, 2, 1,
                0, 3, 2,
                0, 4, 3,
                0, 5, 4,
                0, 6, 5, 
                0, 1, 6
            };

            return mesh;
        }

        private void Start()
        {
            GetComponent<MeshFilter>().mesh = MakeHex();
        }

        private void Update()
        {
            transform.Rotate(new Vector3(0, 1, 0), 1.0f);
        }
    }
}