using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace RogoDigital.Lipsync {
	public class ClipSettingsWindow : ModalWindow {
		private LipSyncClipSetup setup;

		private float length;
		private string transcript;
		private Vector2 scroll;

		private bool willTrim = false;
		private bool soXAvailable = false;

		void OnGUI () {
			GUILayout.Space(20);
			scroll = GUILayout.BeginScrollView(scroll);
			TimeSpan time = TimeSpan.FromSeconds(length);

			int minutes = time.Minutes;
			int seconds = time.Seconds;
			int milliseconds = time.Milliseconds;

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(280));
			EditorGUI.BeginDisabledGroup(setup.clip && !soXAvailable);
			EditorGUI.BeginChangeCheck();
			GUILayout.Label("Duration");
			minutes = EditorGUILayout.IntField(minutes);
			GUILayout.Label("m", EditorStyles.miniLabel);
			seconds = EditorGUILayout.IntField(seconds);
			GUILayout.Label("s", EditorStyles.miniLabel);
			milliseconds = EditorGUILayout.IntField(milliseconds);
			GUILayout.Label("ms", EditorStyles.miniLabel);
			if (EditorGUI.EndChangeCheck() && setup.clip) {
				if(((minutes * 60) + seconds + (milliseconds / 1000f)) < setup.clip.length) {
					TimeSpan fileTime = TimeSpan.FromSeconds(setup.fileLength);
					willTrim = !(fileTime.Minutes == minutes && fileTime.Seconds == seconds && fileTime.Milliseconds == milliseconds);
				} else {
					minutes = time.Minutes;
					seconds = time.Seconds;
					milliseconds = time.Milliseconds;
					EditorUtility.DisplayDialog("Invalid Length", "File length cannot be longer than the length of the audio clip! Enter a time that is equal to or shorter than the audio length.", "Ok");
				}
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			if (setup.clip && !soXAvailable) EditorGUILayout.HelpBox("Cannot Change duration as SoX is not available to trim the audio. Follow the included guide to set up SoX.", MessageType.Warning);
			length = (minutes * 60) + seconds + (milliseconds / 1000f);

			GUILayout.Space(10);
			GUILayout.Label("Transcript");
			transcript = GUILayout.TextArea(transcript, GUILayout.MinHeight(90));

			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(willTrim?"Trim & Save":"Save", GUILayout.MinWidth(100), GUILayout.Height(20))) {
				setup.transcript = transcript;
				if (willTrim) {
					TrimClip(0, length);
				}

				setup.fileLength = length;
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
			GUILayout.EndScrollView();
		}

		void TrimClip (double newStartTime, double newLength) {
			if (soXAvailable) {
				// Times
				float newStartNormalised = 1 - ((setup.fileLength - (float)newStartTime) / setup.fileLength);
				float newEndNormalised = ((float)newStartTime + (float)newLength) / setup.fileLength;

				// Paths
				string originalPathRelative = AssetDatabase.GetAssetPath(setup.clip);
				string originalPathAbsolute = Application.dataPath + "/" + originalPathRelative.Substring("/Assets".Length);

				string newPathRelative = Path.GetDirectoryName(originalPathRelative) + "/" + Path.GetFileNameWithoutExtension(originalPathRelative) + "_Trimmed" + Path.GetExtension(originalPathRelative);
				string newPathAbsolute = Application.dataPath + "/" + newPathRelative.Substring("/Assets".Length);

				string soXPath = EditorPrefs.GetString("LipSync_SoXPath");
				string soXArgs = "\"" + originalPathAbsolute + "\" \"" + newPathAbsolute + "\" trim " + newStartTime + " " + newLength;

				System.Diagnostics.Process process = new System.Diagnostics.Process();
				process.StartInfo.FileName = soXPath;
				process.StartInfo.Arguments = soXArgs;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardError = true;

				process.ErrorDataReceived += (object e, System.Diagnostics.DataReceivedEventArgs outLine) => {
					if (!string.IsNullOrEmpty(outLine.Data)) {
						if (outLine.Data.Contains("FAIL")) {
							Debug.LogError("SoX Audio Trimming Failed: " + outLine.Data);
							process.Close();
						}
					}
				};

				process.Start();
				process.BeginErrorReadLine();
				process.WaitForExit(5000);

				AssetDatabase.Refresh();

				AudioClip newClip = AssetDatabase.LoadAssetAtPath<AudioClip>(newPathRelative);

				// Adjust Marker timings (go backwards so indices don't change)
				float multiplier = 1 / (newEndNormalised - newStartNormalised);
				for (int p = setup.phonemeData.Count - 1; p >= 0; p--) {
					if (setup.phonemeData[p].time < newStartNormalised || setup.phonemeData[p].time > newEndNormalised) {
						setup.phonemeData.RemoveAt(p);
					} else {
						setup.phonemeData[p].time -= newStartNormalised;
						setup.phonemeData[p].time *= multiplier;
					}
				}

				for (int g = setup.gestureData.Count - 1; g >= 0; g--) {
					if (setup.gestureData[g].time < newStartNormalised || setup.gestureData[g].time > newEndNormalised) {
						setup.gestureData.RemoveAt(g);
					} else {
						setup.gestureData[g].time -= newStartNormalised;
						setup.gestureData[g].time *= multiplier;
					}
				}

				for (int e = setup.emotionData.Count - 1; e >= 0; e--) {
					if(setup.emotionData[e].endTime < newStartNormalised || setup.emotionData[e].startTime > newEndNormalised) {
						EmotionMarker em = setup.emotionData[e];
						setup.emotionData.Remove(em);
						setup.unorderedEmotionData.Remove(em);
					} else {
						setup.emotionData[e].startTime -= newStartNormalised;
						setup.emotionData[e].startTime *= multiplier;
						setup.emotionData[e].startTime = Mathf.Clamp01(setup.emotionData[e].startTime);

						setup.emotionData[e].endTime -= newStartNormalised;
						setup.emotionData[e].endTime *= multiplier;
						setup.emotionData[e].endTime = Mathf.Clamp01(setup.emotionData[e].endTime);
					}
				}

				setup.clip = newClip;
				length = setup.clip.length;
				setup.FixEmotionBlends();
			}
		}

		public static ClipSettingsWindow CreateWindow (ModalParent parent, LipSyncClipSetup setup) {
			ClipSettingsWindow window = CreateInstance<ClipSettingsWindow>();

			window.length = setup.fileLength;
			window.transcript = setup.transcript;

			window.position = new Rect(parent.center.x - 250, parent.center.y - 100, 500, 200);
			window.minSize = new Vector2(500, 200);
			window.titleContent = new GUIContent("Clip Settings");

			window.soXAvailable = AutoSync.CheckSoX();
			window.setup = setup;
			window.Show(parent);
			return window;
		}
	}
}