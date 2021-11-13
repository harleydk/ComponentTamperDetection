using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace harleydk.ComponentTamperDetection
{
    /// <summary>
    /// A test MonoBehaviour for the ComponentTamperDetection component. Implements the IComponentTamperDetection-interface and 
    /// thus allows the ComponentTamperDetection to automatically register value-changes.
    /// </summary>
    public class ComponentTamperDetectionDynamicTest : MonoBehaviour, IComponentTamperDetection
    {
        public event Action OnEditorValuesChanged;

        public void OnValidate()
        {
            if (Application.isEditor && OnEditorValuesChanged != null)
                OnEditorValuesChanged.Invoke();
        }

        public bool BoolTest;
        public string StringTest;
        public int IntTest;
        public float FloatTest;
        public Color ColorTest;
        public GameObject GameObjectRefTest;
        public Rect RectTest;
        public Vector3 Vector3Test;
        public Vector2 Vector2Test;
        public List<int> ListTestPrimitive;
        public List<GameObject> ListTestGO;
        public UnityEvent UnityEventTest;

        public void SetFoobarTest(string data)
        {
            StringTest = data;
        }
        public void SetFoobaFloat(float data)
        {
            FloatTest = data;
        }

        public void SetFoobaint(int data)
        {
            IntTest = data;
        }

        public void SetFoobaRect(Rect data)
        {
            RectTest = data;
        }

        public void SetFoobaReffff(GameObject data)
        {
            GameObjectRefTest = data;
        }
    }
}