using System.IO;
using UnityEditor;
using UnityEngine;
using static SomeProject.EditorExtensions.Paths;

namespace SomeProject.EditorExtensions
{
    [InitializeOnLoad]
    public class SetupProjectHelper
    {
        /// <summary>
        /// The project uses `unityyamlmerge` tool to manage scene merges.
        /// In order for git to find the tool, it must be in PATH environment variable.
        /// This editor extension provides an easy way of getting that directory.
        /// </summary>
        [MenuItem("Project Setup/Copy path to UnityYAMLMerge folder")]
        private static void CopyPathToUnityYaml()
        {
            var editorExecutablePath = Path.GetDirectoryName(EditorApplication.applicationPath);
            var editorFolderPath     = Path.Combine(editorExecutablePath, "Data", "Tools");
            GUIUtility.systemCopyBuffer = editorFolderPath;
            Debug.Log("The path has been copied to your clipboard");
        }

        /// <summary>
        /// Copies files from git_hooks into .git/hooks 
        /// </summary>
        [MenuItem("Project Setup/Initialize git hooks")]
        private static void InitializeGitHooks()
        {
            var gitHooksSourceControlPath = Path.Combine(ProjectRootPath, "git_hooks");
            var gitHooksDotGitPath        = Path.Combine(ProjectRootPath, ".git", "hooks");

            foreach (var hookFilePath in Directory.GetFiles(gitHooksSourceControlPath))
            {
                var fileName = Path.GetFileName(hookFilePath);
                var hookFileDestinationPath = Path.Combine(gitHooksDotGitPath, fileName);

                File.Copy(hookFilePath, hookFileDestinationPath, overwrite: true);
                Debug.Log($"Copied {hookFilePath} to {hookFileDestinationPath}");
            }
        }
    }
}
