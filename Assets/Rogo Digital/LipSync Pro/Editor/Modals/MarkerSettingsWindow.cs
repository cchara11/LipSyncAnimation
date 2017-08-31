using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace RogoDigital.Lipsync {
	public class MarkerSettingsWindow : ModalWindow {
		private LipSyncClipSetup setup;
		private int markerType;
		private PhonemeMarker pMarker;
		private EmotionMarker eMarker;
		private Vector2 scrollPosition;
		private AnimBool modifierBool;

		private string[] phonemeNames;
		private int[] phonemeValues;

		private float time;
		private float startTime;
		private float endTime;
		private int phonemeNumber;
		private float intensity;
		private bool modifierOn;
		private float maxVariationFrequency;
		private float intensityModifier;
		private float blendableModifier;
		private float bonePositionModifier;
		private float boneRotationModifier;

		void OnGUI () {
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (markerType == 0) {
				GUILayout.Label("Editing " + setup.settings.phonemeSet.phonemes[pMarker.phonemeNumber].name + " Phoneme Marker at " + (pMarker.time * setup.fileLength).ToString() + "s.");
			} else {
				GUILayout.Label("Editing " + eMarker.emotion + " Emotion Marker at " + (eMarker.startTime * setup.fileLength).ToString() + "s.");
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			if (markerType == 0) {
				time = EditorGUILayout.FloatField("Marker Time", time);
				phonemeNumber = EditorGUILayout.IntPopup("Phoneme", phonemeNumber, phonemeNames, phonemeValues);
			} else {
				startTime = EditorGUILayout.FloatField("Start Time", startTime);
				endTime = EditorGUILayout.FloatField("End Time", endTime);
			}
			GUILayout.Space(10);
			intensity = EditorGUILayout.Slider("Intensity", intensity, 0, 1);
			modifierOn = EditorGUILayout.Toggle(markerType == 0 ? "Use Randomess" : "Use Continuous Variation", modifierOn);
			modifierBool.target = modifierOn;
			if (EditorGUILayout.BeginFadeGroup(modifierBool.faded)) {
				if (markerType == 1) {
					GUILayout.BeginHorizontal();
					maxVariationFrequency = EditorGUILayout.Slider("Vary every:", maxVariationFrequency, 0.2f, 3);
					GUILayout.Label(" seconds");
					GUILayout.EndHorizontal();
				}
				intensityModifier = EditorGUILayout.Slider(markerType == 0 ? "Intensity Randomness" : "Intensity Variation", intensityModifier, 0, 1);
				blendableModifier = EditorGUILayout.Slider(markerType == 0 ? "Blendable Value Randomness" : "Blendable Value Variation", blendableModifier, 0, 1);
				bonePositionModifier = EditorGUILayout.Slider(markerType == 0 ? "Bone Position Randomness" : "Bone Position Variation", bonePositionModifier, 0, 1);
				boneRotationModifier = EditorGUILayout.Slider(markerType == 0 ? "Bone Rotation Randomness" : "Bone Rotation Variation", boneRotationModifier, 0, 1);
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndScrollView();
			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Accept", GUILayout.MinWidth(100), GUILayout.Height(20))) {
				if (markerType == 0) {
					pMarker.time = time;
					pMarker.phonemeNumber = phonemeNumber;
					pMarker.intensity = intensity;

					pMarker.useRandomness = modifierOn;
					pMarker.intensityRandomness = intensityModifier;
					pMarker.blendableRandomness = blendableModifier;
					pMarker.bonePositionRandomness = bonePositionModifier;
					pMarker.boneRotationRandomness = boneRotationModifier;
				} else {
					eMarker.startTime = startTime;
					eMarker.endTime = endTime;
					eMarker.intensity = intensity;

					eMarker.continuousVariation = modifierOn;
					eMarker.variationFrequency = maxVariationFrequency;
					eMarker.intensityVariation = intensityModifier;
					eMarker.blendableVariation = blendableModifier;
					eMarker.bonePositionVariation = bonePositionModifier;
					eMarker.boneRotationVariation = boneRotationModifier;
				}
				setup.changed = true;
				setup.previewOutOfDate = true;
				Close();
			}
			GUILayout.Space(10);
			if (GUILayout.Button("Cancel", GUILayout.MinWidth(100), GUILayout.Height(20))) {
				Close();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(20);
		}

		public static void CreateWindow (ModalParent parent, LipSyncClipSetup setup, PhonemeMarker marker) {
			MarkerSettingsWindow window = Create(parent, setup, 0);
			window.pMarker = marker;

			window.time = marker.time;
			window.phonemeNumber = marker.phonemeNumber;
			window.intensity = marker.intensity;
			window.modifierOn = marker.useRandomness;
			window.intensityModifier = marker.intensityRandomness;
			window.blendableModifier = marker.blendableRandomness;
			window.boneRotationModifier = marker.boneRotationRandomness;
			window.bonePositionModifier = marker.bonePositionRandomness;

			window.modifierBool = new AnimBool(window.modifierOn, window.Repaint);
		}

		public static void CreateWindow (ModalParent parent, LipSyncClipSetup setup, EmotionMarker marker) {
			MarkerSettingsWindow window = Create(parent, setup, 1);
			window.eMarker = marker;

			window.startTime = marker.startTime;
			window.endTime = marker.endTime;
			window.intensity = marker.intensity;
			window.modifierOn = marker.continuousVariation;
			window.maxVariationFrequency = marker.variationFrequency;
			window.intensityModifier = marker.intensityVariation;
			window.blendableModifier = marker.blendableVariation;
			window.boneRotationModifier = marker.boneRotationVariation;
			window.bonePositionModifier = marker.bonePositionVariation;

			window.modifierBool = new AnimBool(window.modifierOn, window.Repaint);
		}

		private static MarkerSettingsWindow Create (ModalParent parent, LipSyncClipSetup setup, int markerType) {
			MarkerSettingsWindow window = CreateInstance<MarkerSettingsWindow>();

			window.phonemeNames = setup.settings.phonemeSet.phonemes.phonemeNames.ToArray();
			window.phonemeValues = new int[window.phonemeNames.Length];

			for (int i = 0; i < window.phonemeValues.Length; i++) {
				window.phonemeValues[i] = i;
			}

			window.position = new Rect(parent.center.x - 250, parent.center.y - 100, 500, 200);
			window.minSize = new Vector2(500, 200);
			window.titleContent = new GUIContent("Marker Settings");

			window.setup = setup;
			window.markerType = markerType;
			window.Show(parent);
			return window;
		}
	}
}