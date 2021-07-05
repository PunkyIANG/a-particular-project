using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static SomeProject.EditorExtensions.Paths;
using Hextant;
using Hextant.Editor;

namespace SomeProject.EditorExtensions
{
    [Settings(SettingsUsage.EditorUser, "Code Generation setting")]
    public sealed class CodeGenerationSettings : Settings<CodeGenerationSettings>
    {
        public bool RegenerateOnReload => _regenerateOnReload;
        [SerializeField, Tooltip("Whether to call Kari when the editor restarts")] 
        private bool _regenerateOnReload = true;

        // public float floatValue => _floatValue;
        // [SerializeField, Range( 0, 100 )] float _floatValue = 25.0f;

        // public string stringValue => _stringValue;
        // [SerializeField, Tooltip( "A string value." )] string _stringValue = "Hello";

        [SettingsProvider]
        static SettingsProvider GetSettingsProvider() => instance.GetSettingsProvider();
    }

    [InitializeOnLoad]
    internal class CodeGenerationManager
    {
        /// <summary>
        /// Invokes Kari from the python CLI.
        /// We define the actual task done is defined, by design, in the python script.
        /// Change the Unity subcommand if you need more control.
        /// </summary>
        [MenuItem("Project Setup/Generate code via Kari")]
        private static void GenerateCode()
        {
            var cliPath = Path.Combine(ProjectRootPath, "cli.py");
            
            // The default working directory is Game, but I believe it, unlike the dataPath, is not stable
            if (!RunProcess("python", $"{cliPath} kari unity", workingDirectory: ProjectRootPath))
            {
                UnityEngine.Debug.LogError("Code Generation failed");
            }
        }

        static CodeGenerationManager()
        {
            if (CodeGenerationSettings.instance.RegenerateOnReload)
                GenerateCode();
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