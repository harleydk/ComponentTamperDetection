using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

namespace harleydk.ComponentTamperDetection
{
    public class ComponentTamperDetection : MonoBehaviour
    {
        public MonoBehaviour ScriptReference;

        [HideInInspector]
        public string LockDateTicks;

        [HideInInspector]
        public bool Locked;

        [HideInInspector]
        public bool ShowInputDebugger = false;

        [HideInInspector]
        public string fieldsAndHashes;
        public Dictionary<string, int> _fieldsAndHashes { get; set; }

        [HideInInspector]
        public string latestHashes;
        public Dictionary<string, int> _latestHashes { get; set; } // shadow values, we can compare changes against

        private bool _isFirstTimeCalled;
        private bool subscribesToChangesInReferredMonobehaviour;

        [HideInInspector]
        public string currentScriptRefId; // we keep a reference to the current scriptref-object, so we can - in OnValidate - check if it has been changed.

        public ComponentTamperDetection()
        {
            Debug.Log("Constructed ComponentTamperDetection object");
            _isFirstTimeCalled = true;
            ShowInputDebugger = true;

        }

        private void addChangeDetectHandlerIfPossible(MonoBehaviour scriptRef)
        {
            bool implementsIComponentTamperDetection = scriptRef.GetType().GetInterfaces().Contains(typeof(IComponentTamperDetection));
            if (!implementsIComponentTamperDetection)
                Debug.LogWarning($"ScriptRef on component {scriptRef.name} on GO {scriptRef.gameObject.name} doesn't implement IComponentTamperDetection. We will not be able to dynamically detect changes.");
            else if (implementsIComponentTamperDetection && !subscribesToChangesInReferredMonobehaviour)
            {
                ((IComponentTamperDetection)scriptRef).OnEditorValuesChanged += ComponentTamperDetection_OnEditorValuesChanged;
                subscribesToChangesInReferredMonobehaviour = true;
            }
        }

        private bool hasComponentChanged()
        {
            long sumOfCurrentValues = _fieldsAndHashes.Sum(fh => (long)fh.Value);
            _latestHashes = calculateHashFromPublicFields();
            latestHashes = serializeDicionary(_latestHashes);
            long sumOfLatestValues = _latestHashes.Sum(lv => (long)lv.Value);

            return sumOfCurrentValues != sumOfLatestValues;
        }

        private void ComponentTamperDetection_OnEditorValuesChanged()
        {
            bool hasChanged = hasComponentChanged();
            if (hasChanged)
            {
                Locked = false;
                LockDateTicks = null;

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        private void OnDestroy()
        {
            bool implementsIComponentTamperDetection = ScriptReference.GetType().GetInterfaces().Contains(typeof(IComponentTamperDetection));
            if (implementsIComponentTamperDetection)
                ((IComponentTamperDetection)ScriptReference).OnEditorValuesChanged -= ComponentTamperDetection_OnEditorValuesChanged;
        }

        /// <summary>
        /// Triggers when the script is loaded into the editor, something is changing the Unity Editor GUI for this component. 
        /// </summary>
        /// <remarks>
        /// OnValidate will be called when the Unity editor loads the scene, with the component inside. Where we utilize 
        /// a _isFirstTimeCalled bool to indicate this, so we can set the 
        /// </remarks>
        public void OnValidate()
        {
            Debug.Log($"OnValidate() called on {this.gameObject.name}-{this.name}");

            if (_isFirstTimeCalled)
            {
                _isFirstTimeCalled = false;
                if (ScriptReference != null)
                {
                    if (!string.IsNullOrWhiteSpace(fieldsAndHashes))
                        _fieldsAndHashes = deserializesDicionary(fieldsAndHashes);

                    if (!string.IsNullOrWhiteSpace(latestHashes))
                        _latestHashes = deserializesDicionary(latestHashes);

                    currentScriptRefId = GetComponentPath(ScriptReference);

                    bool implementsIComponentTamperDetection = ScriptReference.GetType().GetInterfaces().Contains(typeof(IComponentTamperDetection));
                    if (implementsIComponentTamperDetection)
                    {
                        addChangeDetectHandlerIfPossible(ScriptReference);
                    }
                    else
                    {
                        // The provided script does not implement the IComponentTamperDetection, and we will not be able to dynamically register component-changes.
                        // The best we can do, then, is to check for changes in this constructor.
                        bool hasChanged = hasComponentChanged();
                        if (hasChanged)
                        {
                            Locked = false;
                            LockDateTicks = null;
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                }

                return;
            }

            string fds = GetComponentPath(ScriptReference);
            if (ScriptReference is null)
            {
                Debug.LogWarning($"Scriptref on component {this.name} on GO {this.gameObject.name} should not be null.");
                return;
            }
            else if (currentScriptRefId != GetComponentPath(ScriptReference))
            {
                // If a new script is referenced, we must re-calculate the hashes
                Locked = false;
                LockDateTicks = null;

                _fieldsAndHashes = calculateHashFromPublicFields();
                fieldsAndHashes = serializeDicionary(_fieldsAndHashes);
                latestHashes = null;
                _latestHashes = null;

                addChangeDetectHandlerIfPossible(ScriptReference);
                currentScriptRefId = GetComponentPath(ScriptReference);

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        private string serializeDicionary(Dictionary<string, int> dictionary)
        {
            var items = dictionary.Select(d => string.Join(";", d.Key, d.Value));
            string serializedDict = string.Join("§", items);
            return serializedDict;
        }

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
        /// Generates a hash-value for all public properties of
        /// the monobehaviour, i.e. those values we can set in the editor.
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

        private Dictionary<string, int> calculateHashFromPublicFields()
        {
            var fieldsAndHashes = new Dictionary<string, int>();

            var monoBehaviour = ScriptReference;
            var type = monoBehaviour.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var field in fields)
            {
                var objectRef = field.GetValue(ScriptReference);
                if (objectRef is null)
                {
                    fieldsAndHashes.Add(field.Name, 0);
                }
                else if (objectRef is MonoBehaviour)
                {
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
                    int valueHash = objectRef.ToString().GetHashCode();
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
                                // corresponds with a 'None' list-item 
                            }
                        }
                        else if (objectRef.GetType().IsPrimitive)
                            hashCode += objectRef.ToString().GetHashCode();
                    }
                    fieldsAndHashes.Add(field.Name, hashCode);
                }
                else if (objectRef is Component) // fx. Image, or Canvas
                {
                    string gameObjectPath = GetGameObjectPath(((Component)objectRef).transform);
                    fieldsAndHashes.Add(field.Name, string.Join("§", gameObjectPath, ((Component)objectRef).name).GetHashCode());
                }
                else if (objectRef is GameObject)
                {
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

        public static string GetUnityEventValue(object componentWithEvents, string unityEventName)
        {
            List<string> eventHandlerData = new List<string>();

            SerializedObject monobehaviourRef = new SerializedObject((UnityEngine.Object)componentWithEvents);
            SerializedProperty unityEventHandlers = monobehaviourRef.FindProperty(unityEventName).FindPropertyRelative("m_PersistentCalls.m_Calls");

            for (int eventHandlerIndex = 0; eventHandlerIndex < unityEventHandlers.arraySize; eventHandlerIndex++)
            {
                UnityEngine.Object target = unityEventHandlers.GetArrayElementAtIndex(eventHandlerIndex).FindPropertyRelative("m_Target").objectReferenceValue;
                if ( target == null)
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
                } // end if any parameters to this unity event-handler
            }

            return string.Join("§", eventHandlerData);
        }

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

        public static string GetComponentPath(MonoBehaviour monoBehaviour)
        {
            string gameobjectPath = GetGameObjectPath(monoBehaviour.transform);
            string componentName = monoBehaviour.GetType().Name;

            return string.Join("§", gameobjectPath, componentName);
        }
    }
}
