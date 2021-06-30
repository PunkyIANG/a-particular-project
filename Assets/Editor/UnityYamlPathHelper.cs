using System.IO;
using UnityEditor;
using UnityEngine;

namespace DefaultCompany.Test
{
    [InitializeOnLoad]
    public class UnityYamlPathHelper
    {
        /// <summary>
        /// The project uses `unityyamlmerge` tool to manage scene merges.
        /// In order for git to find the tool, it must be in PATH environment variable.
        /// This editor extension provides an easy way of getting that directory.
        /// </summary>
        [MenuItem("Project Setup/Get path to UnityYAMLMerge folder")]
        static void UnityYamlCopyPath()
        {
            var editorFolderPath = $"{Path.GetDirectoryName(EditorApplication.applicationPath)}\\Data\\Tools";
            GUIUtility.systemCopyBuffer = editorFolderPath;
            Debug.Log("The path has been copied to your clipboard");
        }
    }
}
