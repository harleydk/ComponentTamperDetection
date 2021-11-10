using System;
using System.Text;
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
        private GUIStyle lockedGuiStyle = new GUIStyle();
        private GUIStyle notlockedGuiStyle = new GUIStyle();

        public override void OnInspectorGUI()
        {
            // https://answers.unity.com/questions/1154384/labelfield-wont-change-background-color.html
            //iOSAppInBackgroundBehavior-Color: green, red(white text);
            setupRichText();
            
            ComponentTamperDetection tamperDetectionTarget = (ComponentTamperDetection)target;
            DrawDefaultInspector();


            if (tamperDetectionTarget.Locked && !string.IsNullOrWhiteSpace(tamperDetectionTarget.LockDateTicks))
            {
                DateTime dt = new DateTime(Convert.ToInt64(tamperDetectionTarget.LockDateTicks));
                EditorGUILayout.LabelField($"<color=black><b>Locked - { dt.ToShortDateString()} {dt.ToString("HH:mm:ss")}</b></color>", lockedGuiStyle);
            }
            else
                EditorGUILayout.LabelField("<color=white><b>" + "Not locked" + "</b></color>", notlockedGuiStyle); // works

            if (GUILayout.Button("Lock"))
            {
                if (tamperDetectionTarget.ScriptReference == null)
                    EditorUtility.DisplayDialog("Whoops", "Missing script-ref.", "OK");
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

                        GUILayout.Label($"{field.Key}", rt, GUILayout.Width(200));

                        //EditorGUILayout.LabelField($"{field.Value}", leftAligned, GUILayout.Width(100));

                        if (tamperDetectionTarget._latestHashes != null && tamperDetectionTarget._latestHashes.ContainsKey(field.Key))
                        {
                            bool differentHashRegistered = field.Value != tamperDetectionTarget._latestHashes[field.Key];
                            if (differentHashRegistered)
                            {
                                int differentHash = tamperDetectionTarget._latestHashes[field.Key];
                                EditorGUILayout.LabelField($"{differentHash}", leftAlignedRed, GUILayout.Width(100));
                                EditorGUILayout.LabelField($"Changed", rightAlignedRed);
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"{field.Value}", leftAlignedGreen, GUILayout.Width(100));
                                GUILayout.Label("OK", rightAlignedGreen);
                            }
                        }
                        else
                        {
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

        public void setupRichText()
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
            leftAlignedGreen.normal.textColor = Color.blue;
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
            rightAlignedGreen.normal.textColor = Color.blue;


            Texture2D lockedTexture = new Texture2D(2, 2);
            lockedTexture.SetColor(new Color(0, 230, 0, 128));//r,g,b,a 
            lockedGuiStyle.fontStyle = FontStyle.Bold;
            lockedGuiStyle.alignment = TextAnchor.MiddleCenter;
            lockedGuiStyle.richText = true;
            lockedGuiStyle.fontSize = 15;
            lockedGuiStyle.normal.background = lockedTexture;

            Texture2D notLockedTexture = new Texture2D(2, 2);
            notLockedTexture.SetColor(new Color(230, 0, 0, 128));//r,g,b,a 
            notlockedGuiStyle.fontStyle = FontStyle.Bold;
            notlockedGuiStyle.fontSize = 15;
            notlockedGuiStyle.alignment = TextAnchor.MiddleCenter;
            notlockedGuiStyle.normal.background = notLockedTexture;
        }
    }



}