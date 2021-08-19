using EngineCommon.Editor;
using UnityEditor;

namespace SomeProject.Hexagon.Editor
{
    public class HexSaver
    {
        [MenuItem("Hexes/Save Hex Mesh")]
        public static void SaveHexMesh()
        {
            var mesh = BoardManager.MakeHexMesh();
            MeshSaverEditor.SaveMesh(mesh, "Hex", makeNewInstance: true, optimizeMesh: true);
        }
    }
}