using UnityEngine;
using UnityEngine.Events;

namespace harleydk.ComponentTamperDetection
{
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