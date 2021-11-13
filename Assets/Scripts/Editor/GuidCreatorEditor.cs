using UnityEditor;

namespace harleydk.ComponentTamperDetection
{
    /// <summary>
    /// Custom editor for the GuidCreator component. We want to display the Guid in a label, not allowing it to change.
    /// </summary>
    [CustomEditor(typeof(GuidCreator))]
    public class GuidCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GuidCreator guidCreatorScript = (GuidCreator)target;
            DrawDefaultInspector();

            EditorGUILayout.LabelField($"Guid: {guidCreatorScript.Guid}");
        }
    }
}