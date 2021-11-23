using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

namespace harleydk.ComponentTamperDetection
{
    /// <summary>
    /// The ComponentTamperDetection holds a reference to a MonoBehaviour and allows for, by the press of a button, 'locking' its 
    /// public-values in place. Later unforseen changes to these values can then be accounted for.
    /// </summary>
    public class ComponentTamperDetection : MonoBehaviour
    {
        public MonoBehaviour ScriptReference; // Reference to the MonoBehaviour that should be 'locked down'.

        [HideInInspector]
        public bool Locked; // Is the referenced MonoBehaviour locked down?

        [HideInInspector]
        public string LockDateTicks; // When was the referenced Monobehaviour locked?

        [HideInInspector]
        public bool ShowInputDebugger = false; // Should we show, in the editor, the various calculated hash-codes.

        [HideInInspector]
        public string fieldsAndHashes; // A serialized dictionary with field-names and their calculated hash-codes
        public Dictionary<string, int> _fieldsAndHashes { get; set; }

        [HideInInspector]
        public string latestHashes; // A serialized dictionary with field-names and calculated hash-codes, to compare against the locked-down hash-codes.
        public Dictionary<string, int> _latestHashes { get; set; }

        private bool _isFirstTimeCalled; // Set to 'true' the first time the component is loaded - allows us to do some setting-up in the OnValidate() method.

        [HideInInspector]
        public string scriptReferenceId; // we keep a reference to the current MonoBehaviour-reference, so we can - in OnValidate - check if it has been changed to a different one.

        public ComponentTamperDetection()
        {
            _isFirstTimeCalled = true;
            ShowInputDebugger = true;
        }

        /// <summary>
        /// Triggers when the script is loaded into the editor, something is changing the Unity Editor GUI for this component. 
        /// </summary>
        /// <remarks>
        /// OnValidate will be called when the Unity editor loads the scene, with the component inside. Where we utilize 
        /// a _isFirstTimeCalled bool to indicate this, so we can re-populate the internal dictionaries and re-create 
        /// the IComponentTamperDetection-callback, if possible.
        /// </remarks>
        public void OnValidate()
        {
            if (_isFirstTimeCalled)
            {
                if (ScriptReference != null)
                {
                    // It's the first time this component is loaded, and we therefore have some some setup-work to do.
                    if (!string.IsNullOrWhiteSpace(fieldsAndHashes))
                        _fieldsAndHashes = deserializesDicionary(fieldsAndHashes);

                    if (!string.IsNullOrWhiteSpace(latestHashes))
                        _latestHashes = deserializesDicionary(latestHashes);

                    scriptReferenceId = GetComponentPath(ScriptReference) + ScriptReference.GetInstanceID().ToString();

                    addChangeDetectHandlerIfPossible(ScriptReference);

                    // No matter if the provided script implements the IComponentTamperDetection,
                    // we check for changes when the component is first loaded.
                    bool hasChanged = HasComponentChanged();
                    if (hasChanged)
                    {
                        Locked = false;
                        LockDateTicks = null;
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                }

                _isFirstTimeCalled = false;
                return;
            }

            if (ScriptReference is null)
            {
                Debug.LogWarning($"Scriptref on component {this.name} on GO {this.gameObject.name} should not be null.");
                return;
            }

            // If the Monobehaviour-reference has changed, we'll re-calculate.
            string currentScriptRefId = GetComponentPath(ScriptReference) + ScriptReference.GetInstanceID().ToString();
            if (scriptReferenceId != currentScriptRefId)
            {
                // If a new script is referenced, we must re-calculate the hashes
                Locked = false;
                LockDateTicks = null;

                _fieldsAndHashes = calculateHashFromPublicFields();
                fieldsAndHashes = serializeDicionary(_fieldsAndHashes);
                latestHashes = null;
                _latestHashes = null;

                addChangeDetectHandlerIfPossible(ScriptReference);
                scriptReferenceId = currentScriptRefId;

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// If the referenced MonoBehaviour implements the IComponentTamperDetection-interface, we hook on to its OnEditorValuesChanged-event.
        /// </summary>
        /// <see cref="IComponentTamperDetection"/>
        private void addChangeDetectHandlerIfPossible(MonoBehaviour scriptRef)
        {
            bool implementsIComponentTamperDetection = scriptRef.GetType().GetInterfaces().Contains(typeof(IComponentTamperDetection));
            if (!implementsIComponentTamperDetection)
                Debug.LogWarning($"ScriptRef on component {scriptRef.name} on GO {scriptRef.gameObject.name} doesn't implement IComponentTamperDetection. We will not be able to dynamically detect changes.");
            else
                ((IComponentTamperDetection)scriptRef).OnEditorValuesChanged += ComponentTamperDetection_OnEditorValuesChanged;
        }

        /// <summary>
        /// Event-handling for the OnEditorValuesChanged-event, fired (supposedly) by the OnValidate-method of the referenced MonoBehaviour.
        /// Here we check for changes - and 'unlock' the component if anything has in fact changed.
        /// </summary>
        private void ComponentTamperDetection_OnEditorValuesChanged()
        {
            bool hasChanged = HasComponentChanged();
            if (hasChanged)
            {
                Locked = false;
                LockDateTicks = null;

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// Compares the currently locked hash-codes against freshly calculated ones.
        /// </summary>
        /// <returns>true if changes have been made since lock-down, false otherwise</returns>
        public bool HasComponentChanged()
        {
            if (_fieldsAndHashes == null)
            {
                Debug.LogWarning($"ComponentTamperDetection for script-reference {scriptReferenceId} is null - the script likely has no public fields. Did you mean to reference another MonoBehaviour-script?");
                return false;
            }

            long sumOfCurrentValues = _fieldsAndHashes.Sum(fh => (long)fh.Value);
            _latestHashes = calculateHashFromPublicFields();
            latestHashes = serializeDicionary(_latestHashes);
            long sumOfLatestValues = _latestHashes.Sum(lv => (long)lv.Value);

            return sumOfCurrentValues != sumOfLatestValues;
        }

        /// <summary>
        /// Turns a dictionary into a string
        /// </summary>
        private string serializeDicionary(Dictionary<string, int> dictionary)
        {
            var items = dictionary.Select(d => string.Join(";", d.Key, d.Value));
            string serializedDict = string.Join("§", items);
            return serializedDict;
        }

        /// <summary>
        /// Turns a string into a dictionary
        /// </summary>
        private Dictionary<string, int> deserializesDicionary(string fieldsAndHashes)
        {
            string[] items = fieldsAndHashes.Split('§');
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (var item in items)
            {
                string[] itemData = item.Split(';');
                result.Add(itemData[0], Convert.ToInt32(itemData[1]));
            }
            return result;

        }

        /// <summary>
        /// Generates a hash-value for all public fields of the referenced MonoBehaviour, i.e. those values we can set in the editor.
        /// Marks the component as 'locked' and stores calculated hash-codes from those public fields.
        /// </summary>
        public void Lock()
        {
            _latestHashes = null;
            latestHashes = null;
            _fieldsAndHashes = calculateHashFromPublicFields();
            fieldsAndHashes = serializeDicionary(_fieldsAndHashes);

            Locked = true;
            LockDateTicks = DateTime.Now.Ticks.ToString();

            addChangeDetectHandlerIfPossible(ScriptReference);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Locked: {this.Locked}");
        }

        /// <summary>
        /// Iterates all public fields of the referenced MonoBehaviour and populates a dictionary with hash-codes.
        /// </summary>
        private Dictionary<string, int> calculateHashFromPublicFields()
        {
            var fieldsAndHashes = new Dictionary<string, int>();

            var monoBehaviour = ScriptReference;
            var type = monoBehaviour.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var field in fields)
            {
                var objectRef = field.GetValue(ScriptReference);

                try
                {
                    //string name = ((UnityEngine.Object)objectRef).name;
                }
                catch (UnassignedReferenceException ex)
                {
                    // corresponds with a 'None' list-item. No need to add anything here.
                }


                if (objectRef is null)
                {
                    fieldsAndHashes.Add(field.Name, 0);
                }
                else if (objectRef is MonoBehaviour)
                {
                    if (objectRef == null)
                        continue;

                    var GuidCreatorComponent = ((MonoBehaviour)objectRef).gameObject.GetComponent<GuidCreator>();
                    if (GuidCreatorComponent != null)
                    {
                        // this gameobject has a GuidCreator-component, that we can get the better identity from.
                        fieldsAndHashes.Add(field.Name, string.Join("§", GuidCreatorComponent.Guid, ((MonoBehaviour)objectRef).name).GetHashCode());
                    }
                    else
                    {
                        string gameobjectPath = GetGameObjectPath(((MonoBehaviour)objectRef).transform);
                        fieldsAndHashes.Add(field.Name, string.Join("§", gameobjectPath, ((MonoBehaviour)objectRef).name).GetHashCode());
                    }
                }
                else if (objectRef is Color)
                {
                    string colorHex = ((Color)objectRef).ToString();
                    int colorHash = colorHex.GetHashCode();
                    fieldsAndHashes.Add(field.Name, colorHash);
                }
                else if (objectRef.GetType().IsPrimitive /* is float || objectRef is string || objectRef is int || objectRef is Enum */)
                {
                    int valueHash = objectRef.GetHashCode();
                    fieldsAndHashes.Add(field.Name, valueHash);
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
                {
                    int hashCode = (objectRef as IList).Count;
                    foreach (var value in (objectRef as IEnumerable))
                    {
                        if (value == null)
                            hashCode += 0;
                        else if (value is UnityEngine.Object)
                        {
                            try
                            {
                                string name = ((UnityEngine.Object)value).name;
                                hashCode += name.GetHashCode();
                            }
                            catch (UnassignedReferenceException)
                            {
                                // corresponds with a 'None' list-item. No need to add anything here.
                            }
                        }
                        else if (value.GetType().IsPrimitive)
                            hashCode += value.GetHashCode();
                    }
                    fieldsAndHashes.Add(field.Name, hashCode);
                }
                else if (objectRef is Component) // fx. Image, or Canvas
                {
                    if (objectRef == null)
                        continue;

                    try
                    {
                        string gameObjectPath = GetGameObjectPath(((Component)objectRef).transform);
                        fieldsAndHashes.Add(field.Name, string.Join("§", gameObjectPath, ((Component)objectRef).name).GetHashCode());
                    }
                    catch (UnassignedReferenceException)
                    {
                        fieldsAndHashes.Add(field.Name, 0);
                    }
                }
                else if (objectRef is GameObject)
                {
                    try
                    {
                        if (objectRef == null)
                        {
                            fieldsAndHashes.Add(field.Name, 0);
                            continue;
                        }

                        string gameObjectPath = GetGameObjectPath(((GameObject)objectRef).transform);
                        var GuidCreatorComponent = ((GameObject)objectRef).gameObject.GetComponent<GuidCreator>();
                        if (GuidCreatorComponent != null)
                        {
                            // this gameobject has a GuidCreator-component, that we can get the better identity from.
                            fieldsAndHashes.Add(field.Name, GuidCreatorComponent.Guid.GetHashCode());
                        }
                        else
                        {
                            string gameobjectPath = GetGameObjectPath(((GameObject)objectRef).transform);
                            fieldsAndHashes.Add(field.Name, gameobjectPath.GetHashCode());
                        }
                    }
                    catch (UnassignedReferenceException)
                    {
                        fieldsAndHashes.Add(field.Name, 0);
                    }
                }
                else if (objectRef is string)
                {
                    fieldsAndHashes.Add(field.Name, objectRef.ToString().GetHashCode());
                }
                else if (objectRef is Vector3)
                {
                    string v3s = ((Vector3)objectRef).ToString();
                    fieldsAndHashes.Add(field.Name, v3s.GetHashCode());
                }
                else if (objectRef is Rect)
                {
                    string rect = ((Rect)objectRef).ToString();
                    fieldsAndHashes.Add(field.Name, rect.GetHashCode());
                }
                else if (objectRef is Vector2)
                {
                    string v2s = ((Vector2)objectRef).ToString();
                    fieldsAndHashes.Add(field.Name, v2s.GetHashCode());
                }
                else if (objectRef is UnityEvent)
                {
                    var theUnityEvent = ((UnityEvent)objectRef);
                    if (theUnityEvent.GetPersistentEventCount() == 0)
                    {
                        fieldsAndHashes.Add(field.Name, 0);
                    }
                    else
                    {
                        string eventsValue = GetUnityEventValue(ScriptReference, field.Name);
                        fieldsAndHashes.Add(field.Name, eventsValue.GetHashCode());
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not get hash-code for field {field.Name} on MonoBehaviour {ScriptReference.name} | GO {ScriptReference.gameObject.name}");
                }
            }

            return fieldsAndHashes;
        }

        /// <summary>
        /// Builds a composite string from a unity-event handler, i.e. from the event's referenced game-object, method, and any arguments attached.
        /// </summary>
        public static string GetUnityEventValue(object componentWithEvents, string unityEventName)
        {
            List<string> eventHandlerData = new List<string>();

            SerializedObject monobehaviourRef = new SerializedObject((UnityEngine.Object)componentWithEvents);
            SerializedProperty unityEventHandlers = monobehaviourRef.FindProperty(unityEventName).FindPropertyRelative("m_PersistentCalls.m_Calls");

            for (int eventHandlerIndex = 0; eventHandlerIndex < unityEventHandlers.arraySize; eventHandlerIndex++)
            {
                UnityEngine.Object target = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_Target").objectReferenceValue;
                if (target == null)
                {
                    eventHandlerData.Add("unfinishedBusiness");
                    continue;
                }

                eventHandlerData.Add(target.name);

                string methodName = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_MethodName").stringValue;
                MethodInfo method = target.GetType().GetMethod(methodName);
                eventHandlerData.Add(method.Name);

                var eventParams = method.GetParameters();
                if (eventParams.Any())
                {
                    // only deal with the first parameter. Because that's what UnityEvents in the editor allow for.
                    string typeName = method.GetParameters()[0].ParameterType.Name;
                    string value = string.Empty;
                    switch (typeName)
                    {
                        case "Boolean":
                            value = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue.ToString();
                            break;
                        case "String":
                            value = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_Arguments.m_StringArgument").stringValue;
                            break;
                        case "Int32":
                            value = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_Arguments.m_IntArgument").intValue.ToString();
                            break;
                        case "Single":
                            value = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue.ToString();
                            break;
                        case "GameObject":
                            value = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue.name;
                            break;
                        default:
                            Debug.Log($"No type for {typeName}");
                            break;
                    }

                    eventHandlerData.Add(value);
                } // 'end-if' this unity event-handler has arguments.
            }

            return string.Join("§", eventHandlerData);
        }

        /// <summary>
        /// Calculates a full game-object path for a transform
        /// </summary>
        public static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        /// <summary>
        /// Calculates a full path, parent gameobject inclusive, for a given MonoBehaviour.
        /// </summary>
        public static string GetComponentPath(MonoBehaviour monoBehaviour)
        {
            string gameobjectPath = GetGameObjectPath(monoBehaviour.transform);
            string componentName = monoBehaviour.GetType().Name;

            return string.Join("§", gameobjectPath, componentName);
        }

        /// <summary>
        /// We'll remember to disconnect any event handlers we might've added.
        /// </summary>
        private void OnDestroy()
        {
            bool implementsIComponentTamperDetection = ScriptReference.GetType().GetInterfaces().Contains(typeof(IComponentTamperDetection));
            if (implementsIComponentTamperDetection)
                ((IComponentTamperDetection)ScriptReference).OnEditorValuesChanged -= ComponentTamperDetection_OnEditorValuesChanged;
        }

    }
}
