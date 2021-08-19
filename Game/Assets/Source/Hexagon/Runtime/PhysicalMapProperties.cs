using System;
using System.Collections.Generic;
using UnityEngine;
using Kari.Plugins.DataObject;

namespace SomeProject.Hexagon
{
    [Serializable]
    [DataObject]
    public partial class PhysicalBoardProperties
    {
        public float Spacing = 0.05f;
        public ushort Radius = 3;
        public ushort PanelNeckWidth = 1;
        public GameObject HexPrefab;
        public GameObject PanelHexPrefab;

        public float HalfWidth => HexVectorTransformations.sqrt3 * HalfHeight; 
        public float Width => HexVectorTransformations.sqrt3 * Height; 
        public float HalfHeight => 0.5f + Spacing;
        public float Height => 1.0f + Spacing * 2;
    }
}