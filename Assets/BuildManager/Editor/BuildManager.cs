﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace BuildManager.Editor
{
	/// <summary>
	///     Class for managing builds.
	/// </summary>
	public class BuildManager : MonoBehaviour
	{
#if UNITY_2017_1_OR_NEWER
        [MenuItem("Build Manager/Show Build Player Window")]
        private static void DoSHowBuildPlayerWindow()
        {
            BuildPlayerWindow.ShowBuildPlayerWindow();
        }
#endif

		[MenuItem("Build Manager/Build Player")]
		private static void DoBuild()
		{
			BuildPlayer();
		}

		[MenuItem("Build Manager/Build AssetBundles")]
		public static void BuildAssetBundles()
		{
			// Choose the output path according to the build target.
			var outputPath = Path.Combine("AssetBundles", EditorUserBuildSettings.activeBuildTarget.ToString());
			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);

			BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None,
				EditorUserBuildSettings.activeBuildTarget);
		}

		/// <summary>
		///     Builds a Player for the user selected platform.
		/// </summary>
		public static void BuildPlayer()
		{
			if (PlayerSettings.GetApplicationIdentifier(
					 BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)) ==
				 "com.Company.ProductName")
				PlayerSettings.SetApplicationIdentifier(
					BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget),
					"com." + Sanitize(PlayerSettings.companyName) + "." + Sanitize(PlayerSettings.productName));

			var buildPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
				"Builds/" + EditorUserBuildSettings.activeBuildTarget + "/" + PlayerSettings.productName +
				GetFileExtension(EditorUserBuildSettings.activeBuildTarget));

			// Use the array of scenes from EditorBuildSettings, otherwise it builds with only the current scene.
			List<string> scenes = new List<string>();
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				scenes.Add(EditorBuildSettings.scenes[i].path);
			}

			var buildPlayerOptions = new BuildPlayerOptions
			{
				locationPathName = buildPath,
				target = EditorUserBuildSettings.activeBuildTarget,
				targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget),
				options = BuildOptions.ShowBuiltPlayer,
				scenes = scenes.ToArray()
			};
			BuildPipeline.BuildPlayer(buildPlayerOptions);
		}

		/// <summary>
		///     Sanitizes a string for use as part of the application identifier.
		/// </summary>
		/// <param name="input">String input.</param>
		/// <returns>String not starting with a number and not containing non-alphanumeric characters.</returns>
		private static string Sanitize(string input)
		{
			// if input starts with a number, replace the first character with "x".
			var chars = input.ToCharArray();
			if (char.IsDigit(chars[0]))
			{
				chars[0] = 'x';
				input = new string(chars);
			}

			// Remove all non -letters and non-numbers.
			var reg = new Regex("[^A-Za-z0-9']+");
			input = reg.Replace(input, string.Empty);
			return input;
		}

		/// <summary>
		///     Returns a file extension according to the passed BuildTarget.
		/// </summary>
		/// <param name="buildTarget">BuildTarget</param>
		/// <returns>String file extension (".exe", ".apk", "", etc.).</returns>
		private static string GetFileExtension(BuildTarget buildTarget)
		{
			var output = string.Empty;
			switch (buildTarget)
			{
				case BuildTarget.Android:
					output = ".apk";
					break;
				case BuildTarget.StandaloneLinux:
					output = ".x86";
					break;
				case BuildTarget.StandaloneLinux64:
					output = ".x64";
					break;
				case BuildTarget.StandaloneLinuxUniversal:
					output = ".x86_64";
					break;
#if UNITY_2017_3_OR_NEWER
				case BuildTarget.StandaloneOSX:
#else
				case BuildTarget.StandaloneOSXIntel:
				case BuildTarget.StandaloneOSXIntel64:
				case BuildTarget.StandaloneOSXUniversal:
#endif
					output = ".app";
					break;
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					output = ".exe";
					break;

					//default:
					//	throw new ArgumentOutOfRangeException( "buildTarget", buildTarget, null );
			}

			return output;
		}
		/// <summary>
		///     Class for listening for changed to the active build target.
		/// </summary>
		/// <summary>
		///     Class for pre-build processing.
		/// </summary>ary>
		public class PreBuildProcessor : IPreprocessBuild
		{
			public int callbackOrder
			{
				get { return 0; }
			}

			public void OnPreprocessBuild(BuildTarget buildTarget, string buildPath)
			{
				Debug.Log("Build <color=red>started</color> for <b>" + buildTarget + "</b> at " +
						   DateTime.Now.ToString("F") +
						   ".\nBuild location: \"<b>" + buildPath + "</b>\".");
			}
			/// <summary>
			///     Class for post-build processing.
			/// </summary>ary>
			public class PostBuildProcessor : IPostprocessBuild
			{
				public int callbackOrder
				{
					get { return 0; }
				}

				public void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
				{
					Debug.Log("Build <color=green>completed</color> for <b>" + buildTarget + "</b> at " +
							   DateTime.Now.ToString("F") +
							   ".\nBuild location: \"<b>" + buildPath + "</b>\".");
				}
			}

#if UNITY_2017_3_OR_NEWER
			/// <summary>
			/// Class for scene processing.
			/// </summary>
			public class SceneProcessor : IProcessScene
			{
				public int callbackOrder
				{
					get { return 0; }
				}

				public void OnProcessScene(UnityEngine.SceneManagement.Scene scene)
				{
					Debug.Log("SceneProcessor.OnProcessScene <b>" + scene.name + "</b>.");
				}
			}
#endif

		}
	}
}