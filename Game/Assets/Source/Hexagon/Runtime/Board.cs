using System;
using EngineCommon;
using UnityEngine;

namespace SomeProject.Hexagon
{
    public readonly struct HexagonalWorldMap
    {
        public readonly IHexagonalHexMap<GameObject> Map;
        public readonly Transform Parent;

        public HexagonalWorldMap(IHexagonalHexMap<GameObject> map, Transform parent)
        {
            Map = map;
            Parent = parent;
        }

        public void Clear()
        {
            Helper.DestroyChildren(Parent);   
        }

        public GameObject MakeHex(PhysicalBoardProperties props, HexAxial axial)
        {
            var position = (axial - Map.Center).ToWorldCoordinate(props.Height);
            var hex = GameObject.Instantiate(
                props.HexPrefab, position.ZeroZ() + Parent.position, Quaternion.identity, Parent);
            hex.name = axial.ToString();
            return hex;
        }
    }

    public readonly struct HourGlassPanel
    {   
        public readonly HourGlassHexMap<GameObject> Map;
        public readonly Transform Parent;

        public HourGlassPanel(HourGlassHexMap<GameObject> map, Transform parent)
        {
            Map = map;
            Parent = parent;
        }
        public void Clear()
        {
            Helper.DestroyChildren(Parent);   
        }

        public void Reset(PhysicalBoardProperties props)
        {
            Clear();
            Map.ReInitialize(props.PanelNeckWidth, props.Radius);
            var grid = Map.Grid;
            for (int row = 0; row < grid.Length; row++)
            {
                var position = props.PanelNeckWidth;
                var rowArray = grid[row];
                var rowOffset = Math.Abs(row - props.Radius);
                float leftOffset = -(rowOffset + props.PanelNeckWidth - 1) * (props.HalfWidth);
                float topOffset = (row - props.Radius) * 2 * props.Height * 3f/4;

                for (int col = 0; col < rowArray.Length; col++)
                {
                    var left = leftOffset + col * (props.Width);
                    var entity = GameObject.Instantiate(
                        props.PanelHexPrefab, new Vector3(left, topOffset, 0) + Parent.position, Quaternion.identity, Parent);
                    rowArray[col] = entity;
                    entity.name = $"Panel cell at ({row}, {col})";
                }
            }
        }
    }

    public static class Helper
    {
        public static float GetWidth(float height, int diameter)
        {
            return height * diameter * HexVectorTransformations.sqrt3;
        }

        public static void DestroyChildren(Transform parent)
        {
            int childrenCount = parent.childCount;
            for (int i = childrenCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }
    }

    public class Board
    {
        public readonly HexagonalWorldMap Left;
        public readonly HexagonalWorldMap Right;
        public readonly HourGlassPanel Panel;
        public readonly Transform Parent;
        private readonly PhysicalBoardProperties _props;


        public Board(PhysicalBoardProperties props, Transform parent)
        {
            _props = props;
            Parent = parent;
            Left  = MakeWorldMap(nameof(Left));
            Right = MakeWorldMap(nameof(Right));
            Panel = MakePanel();
        }

        public HourGlassPanel MakePanel()
        {
            var map = new HourGlassHexMap<GameObject>(_props.PanelNeckWidth, _props.Radius);
            var panelParent = new GameObject("Panel").transform;
            panelParent.SetParent(Parent);
            return new HourGlassPanel(map, panelParent);
        }

        public HexagonalWorldMap MakeWorldMap(string name)
        {
            var parentGO = new GameObject(name);
            parentGO.transform.SetParent(Parent);
            var map = new HexagonalWrapAroundMap<GameObject>(_props.Radius);
            var worldMap = new HexagonalWorldMap(map, parentGO.transform);
            return worldMap;
        }

        public void OffsetWorldMaps()
        {
            var amount = _props.Width * _props.Radius + (_props.PanelNeckWidth + 1) * _props.HalfWidth;
            Left.Parent.localPosition = new Vector3(-amount, 0, 0);
            Right.Parent.localPosition = new Vector3(amount, 0, 0);
        }

        private void ResetMap(HexagonalWorldMap worldMap)
        {
            worldMap.Clear();
            worldMap.Map.ReInitialize(_props.Radius);
            worldMap.Map.InstantiateEach(axial => worldMap.MakeHex(_props, axial));
        }

        public void ResetPanel()
        {
            Panel.Reset(_props);
        }

        public void Reset()
        {
            OffsetWorldMaps();
            ResetMap(Left);
            ResetMap(Right);
            ResetPanel();
        }
    }
}