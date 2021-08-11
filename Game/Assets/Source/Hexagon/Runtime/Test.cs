using System;
using System.Collections.Generic;
using System.Diagnostics;
using EngineCommon;
using Kari.Plugins.Terminal;
using UnityEngine;

namespace SomeProject.Hexagon
{
    [Serializable]
    public struct TestParams
    {
        public float Spacing;
        public int MapRadius;
        public GameObject HexPrefab;

        public override bool Equals(object obj)
        {
            return obj is TestParams @params &&
                   Spacing == @params.Spacing &&
                   MapRadius == @params.MapRadius &&
                   EqualityComparer<GameObject>.Default.Equals(HexPrefab, @params.HexPrefab);
        }

        public override int GetHashCode()
        {
            int hashCode = -166033139;
            hashCode = hashCode * -1521134295 + Spacing.GetHashCode();
            hashCode = hashCode * -1521134295 + MapRadius.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(HexPrefab);
            return hashCode;
        }

        public static bool operator==(TestParams a, TestParams b)
        {
            return a.Spacing == b.Spacing && a.MapRadius == b.MapRadius 
                && a.HexPrefab == b.HexPrefab;
        }

        public static bool operator!=(TestParams a, TestParams b)
        {
            return !(a == b);
        }
    }

    public static class IndicesOfThings
    {
        public const int TILE = 0;
        public const int LOGIC = 1;
    }

    // [ExecuteInEditMode]
    public class Test : MonoBehaviour
    {
        [SerializeField] private TestParams _params = new TestParams
        {
            Spacing = 0.05f,
            MapRadius = 2
        }; 

        private TestParams _previousParams;
        private HexagonalWrapAroundMap<GameObject> _map;

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

        private GameObject MakeHex(Vector2 position)
        {
            return Instantiate(_params.HexPrefab, position, Quaternion.identity, this.transform);
        }

        private void Start()
        {
            _previousParams = _params;
            if (_params.HexPrefab == null)
            {
                _params.HexPrefab = GetDefaultHex();
            }
            Reset();
        }
        
        [Command]
        public static void ChangeRadius(int radius)
        {
            GameObject.FindObjectOfType<Test>()._params.MapRadius = radius;
        }

        private void Reset()
        {
            _previousParams = _params;
            int childrenCount = transform.childCount;
            for (int i = childrenCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }

            float height = 1.0f + 2 * _params.Spacing;

            var centerOffset = new HexAxial(_params.MapRadius, _params.MapRadius).ToWorldCoordinate(height);

            HexagonalWrapAroundMapSharedGlobals.ReinitializeForMapSize(_params.MapRadius);
            _map = new HexagonalWrapAroundMap<GameObject>(_params.MapRadius,
                axial => MakeHex(axial.ToWorldCoordinate(height) - centerOffset));
            
            // MeasureThings();
        }

        private void MeasureThings()
        {
            var s = Stopwatch.StartNew();
            foreach (var m in transform.GetComponentsInChildren<MeshRenderer>())
            {
                m.enabled = false;
            }
            foreach (var m in transform.GetComponentsInChildren<MeshRenderer>())
            {
                m.enabled = true;
            }
            s.Stop();
            UnityEngine.Debug.Log($"GetComponentsInChildren ({_map.Count}): " + s.Elapsed);

            s.Reset();
            s.Start();
            int childrenCount = transform.childCount;
            for (int i = childrenCount - 1; i >= 0; i--)
            {
                transform.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
            for (int i = childrenCount - 1; i >= 0; i--)
            {
                transform.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            }
            s.Stop();
            UnityEngine.Debug.Log($"GetChild, GetComponent ({_map.Count}): " + s.Elapsed);
        }

        private void Update()
        {
            if (_previousParams != _params)
            {
                Reset();
            }
        }
    }
}