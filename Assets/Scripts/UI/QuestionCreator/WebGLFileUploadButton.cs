using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class WebGLFileUploadButton : MonoBehaviour, IPointerDownHandler
{
    public Action<string> FileUploaded;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
#endif

    public void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UploadFile(gameObject.name, nameof(OnFileUpload), ".xlsx,.csv", false);
#endif
    }

    public void OnFileUpload(string url)
    {
        FileUploaded?.Invoke(url);
    }
}
