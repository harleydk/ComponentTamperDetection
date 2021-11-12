using System;
using UnityEngine;

namespace harleydk.ComponentTamperDetection
{
    /// <summary>
    /// Implementations of this contract must implement Unity's MonoBehaviour.OnValidate and
    /// invoke the OnEditorValuesChanged action when that OnValidate is called.
    /// </summary>
    /// <seealso cref="ComponentTamperDetectionEditor"/>
    /// <seealso cref="ComponentTamperDetection"/>
    /// <remarks>
    /// 
    /// Reference implementation:
    /// ----------------------------
    /// 
    /// public event Action OnEditorValuesChanged;
    /// public void OnValidate() { if (Application.isEditor && OnEditorValuesChanged != null) OnEditorValuesChanged.Invoke(); }
    /// 
    /// </remarks>
    public interface IComponentTamperDetection
    {
        event Action OnEditorValuesChanged;

        void OnValidate();
    }


}