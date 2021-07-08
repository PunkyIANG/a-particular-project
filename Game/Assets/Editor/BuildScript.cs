using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using static SomeProject.EditorExtensions.Paths;
using System.IO;

namespace SomeProject.EditorExtensions
{
	public class BuildPlayerExample : MonoBehaviour
	{
		[MenuItem("Build/Build Windows")]
		public static void Build()
		{
			if (DoWindowsBuild())
				Debug.Log("Build succeeded.");
			else
				Debug.Log("Build failed.");
		}

		public static bool DoWindowsBuild()
		{
			var buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
			buildPlayerOptions.locationPathName = Path.Combine(BuildPath, "win64");
			buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
			buildPlayerOptions.options = BuildOptions.None;

			var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

			return report.summary.result == BuildResult.Succeeded;
		}
	}
}