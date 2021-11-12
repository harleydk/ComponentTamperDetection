using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace harleydk.ComponentTamperDetection
{
    public class AddComponentTamperDetectorsToAll : EditorWindow
    {
        string interfaceName = "<name-of-your-monoBehaviours-marker-interface-here>";
        private string result = string.Empty;

        [MenuItem("GameObject/Add component-tamper detectors to all...", false, 851)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(AddComponentTamperDetectorsToAll));
        }

        public void OnGUI()
        {

            GUILayout.Label("Adds or deletes component-tamper detectors to all components which implements the specific interface.");

            GUILayout.Label("Interface name:");
            interfaceName = GUILayout.TextField(interfaceName);

            if (GUILayout.Button("Add ComponentTamperDetectors to all"))
            {
                AddComponentDetectors(interfaceName);
            }

            if (GUILayout.Button("Delete all ComponentTamperDetectors"))
            {
                DeleteAllComponentDetectors();
            }

            GUILayout.Label(result);
        }

        private void AddComponentDetectors(string interfaceName)
        {
            var monobehavioursWithIDoYouBrainMarkerComponents = new List<MonoBehaviour>();
            var monobehaviours = GameObject.FindObjectsOfType<MonoBehaviour>().Where(mb => mb.GetType().GetInterfaces().Any(i => i.Name == interfaceName));

            if (!monobehaviours.Any())
            {
                EditorUtility.DisplayDialog("No components fond", $"Found no MonoBehaviours with a '{interfaceName} interface", "OK");
                return;
            }

            int addedCounter = 0;
            foreach (var monobehaviour in monobehaviours)
            {
                GameObject go = monobehaviour.gameObject;

                bool componentTamperAlreadyExists = go.GetComponents<ComponentTamperDetection>().Any(component => component.ScriptReference.GetInstanceID() == monobehaviour.GetInstanceID());
                if (!componentTamperAlreadyExists)
                {
                    var tamperDetector = go.AddComponent<ComponentTamperDetection>();
                    tamperDetector.ScriptReference = monobehaviour;

                    tamperDetector.Lock();

                    result += $"correspondingComponentTamperDetector {monobehaviour.name} with instance-id {monobehaviour.GetInstanceID()} added.{System.Environment.NewLine}";
                    addedCounter++;
                }
            }

            result += $"Done. {addedCounter} added.{System.Environment.NewLine}";
        }

        private void DeleteAllComponentDetectors()
        {
            var monobehavioursWithIDoYouBrainMarkerComponents = new List<MonoBehaviour>();
            var monobehaviours = GameObject.FindObjectsOfType<MonoBehaviour>().Where(mb => mb.GetType().GetInterfaces().Any(i => i.Name == interfaceName));
            if ( ! monobehaviours.Any())
            {
                EditorUtility.DisplayDialog("No components fond", $"Found no MonoBehaviours with a '{interfaceName} interface", "OK");
                return;
            }


            int deletedCounter = 0;
            foreach (var monobehaviour in monobehaviours)
            {
                GameObject go = monobehaviour.gameObject;

                var correspondingComponentTamperDetector = go.GetComponents<ComponentTamperDetection>().Where(component => component.ScriptReference.GetInstanceID() == monobehaviour.GetInstanceID());
                foreach (var tamperDetector in correspondingComponentTamperDetector)
                {
                    DestroyImmediate(tamperDetector);
                    result += $"correspondingComponentTamperDetector {monobehaviour.name} with instance-id {monobehaviour.GetInstanceID()} deleted.{System.Environment.NewLine}";
                    deletedCounter++;
                }
            }

            result += $"Done. {deletedCounter} deleted.{System.Environment.NewLine}";
        }
    }

}

