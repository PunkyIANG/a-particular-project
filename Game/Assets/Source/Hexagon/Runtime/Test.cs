using System;
using EngineCommon;
using UnityEngine;

namespace SomeProject.Hexagon
{
    // [ExecuteInEditMode]
    public class Test : MonoBehaviour
    {
        [SerializeField] private float _spacing = 0.0f;
        [SerializeField] private int _mapDiameter = 3;
        [SerializeField] private GameObject _hexPrefab; 

        private static readonly float sqrt3 = Mathf.Sqrt(3);
        private static readonly float sqrt3_2 = Mathf.Sqrt(3) / 2.0f;
        private static readonly float sqrt3_4 = Mathf.Sqrt(3) / 4.0f;

        private GameObject GetDefaultHex()
        {
            var gm = new GameObject();
            var filter = gm.AddComponent<MeshFilter>();
            var renderer = gm.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            
            mesh.vertices = new Vector3[] 
            { 
                new Vector3(sqrt3_2, -0.5f, 0),
                new Vector3(0, -1.0f, 0),
                new Vector3(-sqrt3_2, -0.5f, 0),
                new Vector3(-sqrt3_2, 0.5f, 0),
                new Vector3(0, 1.0f, 0),
                new Vector3(sqrt3_2, 0.5f, 0),
            };
            mesh.triangles = new int[]
            {
                0, 1, 5,
                1, 4, 5,
                1, 2, 4,
                4, 2, 3
            };

            filter.mesh = mesh;

            return gm;
        }

        private void MakeHex(Vector2 position)
        {
            var obj = Instantiate(_hexPrefab, position, Quaternion.identity);
            obj.transform.SetParent(this.transform);
        }

        private void Start()
        {
            if (_hexPrefab == null)
            {
                _hexPrefab = GetDefaultHex();
            }
            
            int childs = transform.childCount;
            for (int i = childs - 1; i > 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }

            float inradius = sqrt3_2;
            float circumradius = 1;
            float minDiameter = inradius * 2;
            float maxDiameter = circumradius * 2;
            float yIncrement = (maxDiameter * 3) / 4.0f + _spacing;
            float xIncrement = (inradius + _spacing / 2);
            int finalRow = _mapDiameter - (_mapDiameter + 1) / 2;
            float leftMostX = -xIncrement * finalRow;
            Vector2 position = new Vector2(leftMostX, -finalRow * yIncrement);

            for (int row = -finalRow; row <= finalRow; row++) // 1, 2, 3, 2, 1
            {
                // The number of hexes on this row
                int colCount = _mapDiameter - Math.Abs(row);
                
                for (int col = 0; col < colCount; col++)
                {
                    MakeHex(position);
                    position.x += minDiameter + _spacing;
                }
                if (row < 0)
                {
                    leftMostX -= xIncrement;
                }
                else
                {
                    leftMostX += xIncrement;
                }
                position.x = leftMostX;
                position.y += yIncrement;
            }
        }
    }
}