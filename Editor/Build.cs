using UnityEditor;
using UnityEngine;

public class Build 
{
	[MenuItem("Build/Build Windows")]
	public static void BuildPCPlayer()
	{
		string path = $"{Application.dataPath}/../Build/Windows";
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/DiscSpace.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
	}

	[MenuItem("Build/Build Web")]
	public static void BuildWebPlayer()
	{
		string path = $"{Application.dataPath}/../Build/Web";
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, BuildTarget.WebGL, BuildOptions.None);
	}
}