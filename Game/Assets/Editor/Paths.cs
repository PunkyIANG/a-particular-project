using System.IO;
using UnityEngine;

namespace SomeProject.EditorExtensions
{
    public static class Paths
    {
        /// <summary>
        /// SomeProject/Game/Assets
        /// </summary>
        public static readonly string AssetsFolder     = Application.dataPath;
        /// <summary>
        /// SomeProject/Game
        /// </summary>
        public static readonly string UnityProjectPath = Path.Combine(Application.dataPath, "..");
        /// <summary>
        /// SomeProject
        /// </summary>
        public static readonly string ProjectRootPath  = Path.Combine(UnityProjectPath, "..");
    }
}