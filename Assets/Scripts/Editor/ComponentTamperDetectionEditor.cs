using System;
using UnityEditor;
using UnityEngine;

namespace harleydk.ComponentTamperDetection
{
    [CustomEditor(typeof(ComponentTamperDetection))]
    public class ComponentTamperDetectionEditor : Editor
    {
        private GUIStyle rt;
        private GUIStyle bold;
        private GUIStyle leftAlignedGreen;
        private GUIStyle leftAlignedRed;
        private GUIStyle rightAlignedRed;
        private GUIStyle rightAlignedGreen;
        private GUIStyle lockedGuiStyle;
        private GUIStyle notlockedGuiStyle;

        public override void OnInspectorGUI()
        {
            setupGuiStyles();
            DrawDefaultInspector();

            ComponentTamperDetection tamperDetectionTarget = (ComponentTamperDetection)target;
            if (tamperDetectionTarget.ScriptReference != null && tamperDetectionTarget.Locked && !string.IsNullOrWhiteSpace(tamperDetectionTarget.LockDateTicks))
            {
                DateTime dt = new DateTime(Convert.ToInt64(tamperDetectionTarget.LockDateTicks));
                EditorGUILayout.LabelField($"<color=black><b>Locked - { dt.ToShortDateString()} {dt.ToString("HH:mm:ss")}</b></color>", lockedGuiStyle);
            }
            else
                EditorGUILayout.LabelField("<color=white><b>" + "Not locked" + "</b></color>", notlockedGuiStyle); // works

            if (GUILayout.Button("Lock"))
            {
                if (tamperDetectionTarget.ScriptReference == null)
                    EditorUtility.DisplayDialog("Missing reference", "Cannot lock values - there's no referenced MonoBehaviour added.", "My bad");
                else
                    tamperDetectionTarget.Lock();
            }

            EditorGUILayout.Separator();

            // Currently showing all
            if (tamperDetectionTarget.ShowInputDebugger)
            {
                // Button to Hide all
                if (GUILayout.Button("Hide component status"))
                {
                    tamperDetectionTarget.ShowInputDebugger = false;
                }

                if (!string.IsNullOrWhiteSpace(tamperDetectionTarget.fieldsAndHashes))
                {
                    //https://stackoverflow.com/questions/54532110/to-make-last-editorguilayout-of-editorwindow-fill-the-remaining-space
                    GUILayout.FlexibleSpace();
                    foreach (var field in tamperDetectionTarget._fieldsAndHashes)
                    {
                        GUILayout.BeginHorizontal();

                        if (tamperDetectionTarget._latestHashes != null && tamperDetectionTarget._latestHashes.ContainsKey(field.Key))
                        {
                            bool differentHashRegistered = field.Value != tamperDetectionTarget._latestHashes[field.Key];
                            if (differentHashRegistered)
                            {
                                GUILayout.Label($"{field.Key}", leftAlignedRed, GUILayout.Width(200));
                                int differentHash = tamperDetectionTarget._latestHashes[field.Key];
                                EditorGUILayout.LabelField($"{differentHash}", leftAlignedRed, GUILayout.Width(100));
                                EditorGUILayout.LabelField($"Changed", rightAlignedRed);
                            }
                            else
                            {
                                GUILayout.Label($"{field.Key}", rt, GUILayout.Width(200));
                                EditorGUILayout.LabelField($"{field.Value}", leftAlignedGreen, GUILayout.Width(100));
                                GUILayout.Label("OK", rightAlignedGreen);
                            }
                        }
                        else
                        {
                            GUILayout.Label($"{field.Key}", rt, GUILayout.Width(200));
                            EditorGUILayout.LabelField($"{field.Value}", leftAlignedGreen, GUILayout.Width(100));
                            GUILayout.Label("OK", rightAlignedGreen);
                        }

                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Display component status"))
                    tamperDetectionTarget.ShowInputDebugger = true;
            }
        }

        public void setupGuiStyles()
        {
            // Set up our default, rich text label
            if (rt == null)
            {
                rt = new GUIStyle(EditorStyles.label);
            }
            rt.richText = true;
            rt.fontStyle = FontStyle.Normal;

            if (bold == null)
            {
                bold = new GUIStyle(EditorStyles.label);
            }
            bold.fontStyle = FontStyle.Bold;
            bold.richText = true;

            if (leftAlignedGreen == null)
            {
                leftAlignedGreen = new GUIStyle(EditorStyles.label);
            }
            leftAlignedGreen.alignment = TextAnchor.LowerLeft;
            leftAlignedGreen.richText = true;

            if (leftAlignedRed == null)
            {
                leftAlignedRed = new GUIStyle(EditorStyles.label);
            }
            leftAlignedRed.alignment = TextAnchor.LowerLeft;
            leftAlignedRed.normal.textColor = Color.red;
            leftAlignedRed.richText = true;


            if (rightAlignedRed == null)
            {
                rightAlignedRed = new GUIStyle(EditorStyles.label);
            }
            rightAlignedRed.richText = true;
            rightAlignedRed.alignment = TextAnchor.LowerLeft;
            rightAlignedRed.fontStyle = FontStyle.Italic;
            rightAlignedRed.normal.textColor = Color.red;


            if (rightAlignedGreen == null)
            {
                rightAlignedGreen = new GUIStyle(EditorStyles.label);
            }
            rightAlignedGreen.richText = true;
            rightAlignedGreen.alignment = TextAnchor.LowerLeft;
            rightAlignedGreen.fontStyle = FontStyle.Italic;

            if (lockedGuiStyle == null)
            {
                lockedGuiStyle = new GUIStyle();
            }
            Texture2D lockedTexture = new Texture2D(2, 2);
            lockedTexture.SetColor(new Color(0, 230, 0, 128));//r,g,b,a 
            lockedGuiStyle.fontStyle = FontStyle.Bold;
            lockedGuiStyle.alignment = TextAnchor.MiddleCenter;
            lockedGuiStyle.richText = true;
            lockedGuiStyle.fontSize = 15;
            lockedGuiStyle.normal.background = lockedTexture;

            if (notlockedGuiStyle == null)
            {
                notlockedGuiStyle = new GUIStyle();
            }
            Texture2D notLockedTexture = new Texture2D(2, 2);
            notLockedTexture.SetColor(new Color(230, 0, 0, 128));//r,g,b,a 
            notlockedGuiStyle.fontStyle = FontStyle.Bold;
            notlockedGuiStyle.fontSize = 15;
            lockedGuiStyle.richText = true;
            notlockedGuiStyle.alignment = TextAnchor.MiddleCenter;
            notlockedGuiStyle.normal.background = notLockedTexture;
        }
    }
}