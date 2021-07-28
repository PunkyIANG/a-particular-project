using System.Diagnostics;
using UnityEditor;
using static SomeProject.EditorExtensions.Paths;

namespace SomeProject.EditorExtensions
{
    [InitializeOnLoad]
    internal class CodeGenerationManager
    {
        /// <summary>
        /// Invokes Kari from the python CLI.
        /// We define the actual task done, by design, in the python script.
        /// Change the `unity` subcommand if you need more control.
        /// </summary>
        [MenuItem("Project Setup/Generate code via Kari")]
        private static void GenerateCode()
        {
            // The default working directory is Game, but I believe it, unlike the dataPath, is not stable
            if (!RunProcess("baton", "kari unity", workingDirectory: ProjectRootPath))
            {
                UnityEngine.Debug.LogError("Code Generation failed");
            }
        }

        static CodeGenerationManager()
        {
            // if (CodeGenerationSettings.instance.RegenerateOnReload)
            //     GenerateCode();
        }

        private static void LogIfNotNullOrEmpty(string data)
        {
            if (data != null && data != string.Empty)
                UnityEngine.Debug.Log(data);
        }

        private static bool RunProcess(string processName, string args, string workingDirectory)
        {
            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo
                {
                    Arguments = args,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory,
                    FileName = processName
                };
                p.Start();
                
                LogIfNotNullOrEmpty(p.StandardOutput.ReadToEnd());
                LogIfNotNullOrEmpty(p.StandardError.ReadToEnd());

                if (p.ExitCode != 0)
                    return false;
            }
            return true;
        }
    }
}