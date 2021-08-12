using System;
using System.Collections.Generic;
using UnityEngine;

namespace SomeProject.Hexagon
{
    [Serializable]
    public class PhysicalBoardProperties
    {
        public float Spacing = 0.05f;
        public int Radius = 3;
        public GameObject HexPrefab;
        public GameObject PanelHexPrefab;
        public int PanelNeckWidth = 1;

        public float HalfWidth => HexVectorTransformations.sqrt3 * HalfHeight; 
        public float Width => HexVectorTransformations.sqrt3 * Height; 
        public float HalfHeight => 0.5f + Spacing;
        public float Height => 1.0f + Spacing * 2;

        public PhysicalBoardProperties Copy => (PhysicalBoardProperties) MemberwiseClone();
        // TODO: Manage this per property to not do stupid stuff like this
        public void Sync(PhysicalBoardProperties other)
        {
            this.Spacing = other.Spacing;
            this.Radius = other.Radius;
            this.HexPrefab = other.HexPrefab;
            this.PanelHexPrefab = other.PanelHexPrefab;
            this.PanelNeckWidth = other.PanelNeckWidth;
        }

        public override bool Equals(object obj)
        {
            return obj is PhysicalBoardProperties props && props == this;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -166033139;
                hashCode = hashCode * -1521134295 + Spacing.GetHashCode();
                hashCode = hashCode * -1521134295 + Radius.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(HexPrefab);
                return hashCode;
            }
        }

        public static bool operator==(PhysicalBoardProperties a, PhysicalBoardProperties b)
        {
            return a.Spacing == b.Spacing && a.Radius == b.Radius 
                && a.HexPrefab == b.HexPrefab 
                && a.PanelNeckWidth == b.PanelNeckWidth 
                && a.PanelHexPrefab == b.PanelHexPrefab;
        }

        public static bool operator!=(PhysicalBoardProperties a, PhysicalBoardProperties b)
        {
            return !(a == b);
        }
    }
}