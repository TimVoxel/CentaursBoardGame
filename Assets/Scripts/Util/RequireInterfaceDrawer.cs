using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#nullable enable

[CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
public class RequireInterfaceDrawer : PropertyDrawer
{
    private RequireInterfaceAttribute RequireInterfaceAttribute => (RequireInterfaceAttribute)attribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var requiredInterfaceType = RequireInterfaceAttribute.InterfaceType;
        EditorGUI.BeginProperty(position, label, property);

        if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
        {
            DrawArrayField(position, property, label, requiredInterfaceType);
        }
        else
        {
            DrawInterfaceObjectField(position, property, label, requiredInterfaceType);
        }

        EditorGUI.EndProperty();
        var args = new InterfaceArgs(GetTypeOrElementType(fieldInfo.FieldType), requiredInterfaceType);
        InterfaceReferenceDrawerUtil.Draw(position, property, label, args);
    }

    private void DrawArrayField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
    {
        var lineHeight = EditorGUIUtility.singleLineHeight;
        property.arraySize = EditorGUI.IntField(new Rect(position.x, position.y, position.width, lineHeight), $"{label.text} Size", property.arraySize);

        for (var i = 0; i < property.arraySize; i++)
        {
            var element = property.GetArrayElementAtIndex(i);
            var elementPosition = new Rect(position.x, position.y + lineHeight * (i + 1), position.width, lineHeight);
            DrawInterfaceObjectField(elementPosition, element, new GUIContent($"Element {i}"), interfaceType);
        }
    }


    private void DrawInterfaceObjectField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
    {
        var oldReference = property.objectReferenceValue;
        var baseType = GetAssignableBaseType(fieldInfo.FieldType, interfaceType);
        var newReference = EditorGUI.ObjectField(position,
                                                    label,
                                                    oldReference,
                                                    typeof(UnityEngine.Object),
                                                    allowSceneObjects: true);

        if (newReference != null && newReference != oldReference)
        {
            ValidateAndAssignObject(property, newReference, interfaceType);
        }
        else if (newReference == null)
        {
            property.objectReferenceValue = null;
        }
    }

    private void ValidateAndAssignObject(SerializedProperty property, UnityEngine.Object newReference, Type interfaceType)
    {
        if (newReference is GameObject gameObject)
        {
            var component = gameObject.GetComponent(interfaceType);

            if (component != null)
            {
                property.objectReferenceValue = component;
                return;
            }
        }
        else if (interfaceType.IsAssignableFrom(newReference.GetType()))
        {
            property.objectReferenceValue = newReference;
            return;
        }

        Debug.LogWarning($"The assigned object does not implement '{interfaceType.Name}'.");
        property.objectReferenceValue = null;
    }

    private Type GetAssignableBaseType(Type fieldType, Type interfaceType)
    {
        var elementType = fieldType.IsArray ? fieldType.GetElementType() :
            fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)
                ? fieldType.GetGenericArguments()[0]
                : fieldType;

        if (interfaceType.IsAssignableFrom(elementType))
        {
            return elementType;
        }

        if (typeof(ScriptableObject).IsAssignableFrom(elementType)) 
        {
            return typeof(ScriptableObject);
        }
        if (typeof(MonoBehaviour).IsAssignableFrom(elementType))
        {
            return typeof(MonoBehaviour);
        }

        return typeof(UnityEngine.Object);
    }

    private Type GetTypeOrElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }
        else if (type.IsGenericType)
        {
            return type.GetGenericArguments().First();
        }
        return type;
    }
}