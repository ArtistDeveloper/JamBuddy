using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public static class UnityEventExtensions
{
    /// <summary>
    /// UnityEvent�� �������� �α׷� ����ϴ� UnityAction�� ����մϴ�.
    /// </summary>
    /// <param name="unityEvent">UnityEvent �ν��Ͻ�</param>
    /// <param name="eventName">UnityEvent ������ (nameof�� ���)</param>
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