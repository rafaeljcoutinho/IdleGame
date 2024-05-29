using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityToolbarExtender.Examples
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold
			};
		}
	}
	
	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
		private const string KEY = "SCENE_SWITCHER_ORIGINAL_SCENE";
		static SceneSwitchLeftButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
			EditorApplication.playModeStateChanged += LogPlayModeState;			
		}

		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();

			if(GUILayout.Button(new GUIContent(">", "Start Scene Splash"), ToolbarStyles.commandButtonStyle))
			{
				var originalScene = EditorSceneManager.GetActiveScene().name;
				EditorPrefs.SetString(KEY, originalScene);
				SceneHelper.StartScene("Splash", true);
			}
		}
		
		private static void LogPlayModeState(PlayModeStateChange obj)
		{
			var originalScene = EditorPrefs.GetString(KEY, null);
			if (obj == PlayModeStateChange.EnteredEditMode && !string.IsNullOrEmpty(originalScene))
			{
				Debug.Log("Returning to original scene: " + originalScene);
				SceneHelper.StartScene(originalScene, false);
				EditorPrefs.DeleteKey(KEY);
			}
		}
	}

	static class SceneHelper
	{
		static string sceneToOpen;
		private static bool play;

		public static void StartScene(string sceneName, bool play)
		{
			SceneHelper.play = play;
			if(EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
			}

			sceneToOpen = sceneName;
			EditorApplication.update += OnUpdate;
		}

		static void OnUpdate()
		{
			if (sceneToOpen == null ||
			    EditorApplication.isPlaying || EditorApplication.isPaused ||
			    EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				Debug.Log("early retrn");
				return;
			}

			EditorApplication.update -= OnUpdate;

			if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				// need to get scene via search because the path to the scene
				// file contains the package version so it'll change over time
				string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
				if (guids.Length == 0)
				{
					Debug.LogWarning("Couldn't find scene file");
				}
				else
				{
					string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
					EditorSceneManager.OpenScene(scenePath);
					if (play)
						EditorApplication.isPlaying = true;
				}
			}
			sceneToOpen = null;
		}
	}
}