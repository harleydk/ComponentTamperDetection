using UnityEngine;
using UnityEngine.Events;

namespace harleydk.ComponentTamperDetection
{
    /// <summary>
    /// A test MonoBehaviour for the ComponentTamperDetection component. Doen't implement the IComponentTamperDetection-interface and 
    /// there does not allows the ComponentTamperDetection to automatically register value-changes - they become evident upon scene load.
    /// </summary>
    public class ComponentTamperDetectionNonDynamicTest : MonoBehaviour
    {
        public bool BoolTest;
        public string StringTest;
        public int IntTest;
        public float FloatTest;
        public Color ColorTest;
        public GameObject GameObjectRefTest;
        public Rect RectTest;
        public Vector3 Vector3Test;
        public Vector2 Vector2Test;
        public UnityEvent UnityEventTest;

        public Transform TransformTest;
        public RectTransform RectTransformTest;
        public Transform BookLoadoutInitialTransform;

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