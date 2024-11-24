using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public static class UnityEventExtensions
{
    /// <summary>
    /// UnityEvent에 변수명을 로그로 출력하는 UnityAction을 등록합니다.
    /// </summary>
    /// <param name="unityEvent">UnityEvent 인스턴스</param>
    /// <param name="eventName">UnityEvent 변수명 (nameof을 사용)</param>
    public static void RegisterLogNameCallback(this UnityEvent unityEvent, MonoBehaviour owner, bool debug = true)
    {
        unityEvent.AddListener(() => PrintLogName(unityEvent, owner, debug));
    }

    private static void PrintLogName(UnityEvent unityEvent, MonoBehaviour owner, bool debug)
    {
        if (!debug)
        {
            return;
        }

        if (unityEvent == null || owner == null)
        {
            Debug.LogWarning("UnityEvent or owner is null.");
            return;
        }

        Type ownerType = owner.GetType();
        FieldInfo[] fields = ownerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(UnityEvent))
            {
                var value = field.GetValue(owner);
                if (value == unityEvent)
                {
                    if (debug)
                    {
                        Debug.Log($"UnityEvent Variable Name: {field.Name}");
                    }
                    return;
                }
            }
        }

        Debug.LogWarning("UnityEvent variable name not found.");
    }
}