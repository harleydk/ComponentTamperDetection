using UnityEditor;

namespace harleydk.ComponentTamperDetection
{
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