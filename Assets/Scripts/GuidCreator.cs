using UnityEngine;

namespace harleydk.ComponentTamperDetection
{
    public class GuidCreator : MonoBehaviour
    {
        public string Guid { get; set; }

        public void generateGuid()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time. This function is only called in editor mode. Reset is most commonly used to give good default values in the Inspector.
        /// </summary>
        private void Reset()
        {
            this.Guid = System.Guid.NewGuid().ToString();
        }
    }


}

