using Kari.Plugins.Terminal;
using UnityEngine;

namespace SomeProject.Hexagon
{
    public static class IndicesOfThings
    {
        public const int TILE = 0;
        public const int LOGIC = 1;
    }

    // [ExecuteInEditMode]
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private PhysicalBoardProperties _props;

        private PhysicalBoardProperties _previousParams;
        private Board _board;

        private static readonly float sqrt3 = Mathf.Sqrt(3);
        private static readonly float sqrt3_2 = Mathf.Sqrt(3) / 2.0f;
        private static readonly float sqrt3_4 = Mathf.Sqrt(3) / 4.0f;

        public static Mesh MakeHexMesh()
        {
            var mesh = new Mesh();
            
            mesh.vertices = new Vector3[] 
            { 
                new Vector3(sqrt3_2, 0.5f, 0),
                new Vector3(sqrt3_2, -0.5f, 0),
                new Vector3(0, -1.0f, 0),
                new Vector3(-sqrt3_2, -0.5f, 0),
                new Vector3(-sqrt3_2, 0.5f, 0),
                new Vector3(0, 1.0f, 0),
            };
            mesh.triangles = new int[]
            {
                0, 1, 2,
                2, 5, 0,
                2, 3, 4,
                2, 4, 5
            };
            mesh.normals = new Vector3[]
            {
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
            };

            return mesh;
        }

        private GameObject GetDefaultHex()
        {
            var gm = new GameObject();
            var filter = gm.AddComponent<MeshFilter>();
            var renderer = gm.AddComponent<MeshRenderer>();
            filter.mesh = MakeHexMesh();
            return gm;
        }

        private void Start()
        {
            _previousParams = _props.Copy;
            if (_props.HexPrefab == null)
            {
                _props.HexPrefab = GetDefaultHex();
            }

            var boardContainer = new GameObject("Board #1").transform;
            boardContainer.SetParent(this.transform);
            _board = new Board(_props, boardContainer);
            _board.Reset();
        }

        private void Update()
        {
            if (_previousParams != _props)
            {
                _board.Reset();
                _previousParams.Sync(_props);
            }
        }
        
        [Command]
        public static void ChangeRadius(ushort radius)
        {
            GameObject.FindObjectOfType<BoardManager>()._props.Radius = radius;
        }
    }
}