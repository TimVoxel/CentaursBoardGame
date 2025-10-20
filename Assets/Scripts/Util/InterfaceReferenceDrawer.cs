using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#nullable enable

[CustomPropertyDrawer(typeof(InterfaceReference<>))]
[CustomPropertyDrawer(typeof(InterfaceReference<,>))]
public class InterfaceReferenceDrawer : PropertyDrawer
{
	private const string ValueFieldName = "_value";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var underlyingProperty = property.FindPropertyRelative(ValueFieldName);
        var args = GetArguments(fieldInfo);

        EditorGUI.BeginProperty(position, label, property);
        var assignedObject = EditorGUI.ObjectField(position, 
                                                    label,
                                                    underlyingProperty.objectReferenceValue,
                                                    typeof(UnityEngine.Object),
                                                    allowSceneObjects: true);

        if (assignedObject != null)
        {
            if (assignedObject is GameObject gameObject)
            {
                ValidateAndAssignObject(underlyingProperty, gameObject.GetComponent(args.InterfaceType), gameObject.name, args.InterfaceType.Name);
            }
            else
            {
                ValidateAndAssignObject(underlyingProperty, assignedObject, args.InterfaceType.Name);
            }
        }
        else
        {
            underlyingProperty.objectReferenceValue = null;
        }

        EditorGUI.EndProperty();
        InterfaceReferenceDrawerUtil.Draw(position, underlyingProperty, label, args);
    }

    private static InterfaceArgs GetArguments(FieldInfo info)
    {
        Type? objectType = null;
        Type? interfaceType = null;
        var fieldType = info.FieldType;

        static bool TryGetTypesFromInterfaceReference(Type type, [MaybeNullWhen(false)] out Type objType, [MaybeNullWhen(false)] out Type ifType)
        {
            objType = null;
            ifType = null;

            if (type?.IsGenericType != true)
            {
                return false;
            }

            var genericType = type.GetGenericTypeDefinition();

            if (genericType == typeof(InterfaceReference<>))
            {
                //If there is only one type parameter, that means that we are using the derived version with TO as UnityEngine.Object
                //Therefore get base type
                type = type.BaseType;
            }

            if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
            {
                var types = type.GetGenericArguments();
                ifType = types[0];
                objType = types[1];
                return true;
            }
            return false;
        }

        static void GetTypesFromList(Type type, out Type? objType, out Type? ifType)
        {
            objType = null;
            ifType = null;

            var listInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));

            if (listInterface != null)
            {
                var elementType = listInterface.GetGenericArguments().First();
                TryGetTypesFromInterfaceReference(elementType, out objType, out ifType);
            }
        }

        if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
        {
            GetTypesFromList(fieldType, out objectType, out interfaceType);
        }

        return new InterfaceArgs(objectType!, interfaceType!);
    }

    private static void ValidateAndAssignObject(SerializedProperty property,
                                        UnityEngine.Object? targetObject,
                                        string componentNameOrType,
                                        string? interfaceName = null)
    {
        if (targetObject != null)
        {
            property.objectReferenceValue = targetObject;
        }
        else
        {
            Debug.LogWarning(@$"The {(interfaceName != null
                ? $"GameObject '{componentNameOrType}'"
                : $"assigned object")} does not have a component that implements '{interfaceName}'.");
            property.objectReferenceValue = null;
        }
    }
}

public struct InterfaceArgs
{
	public Type ObjectType { get; }
	public Type InterfaceType { get; }

	public InterfaceArgs(Type objectType, Type interfaceType)
	{
		Debug.Assert(typeof(UnityEngine.Object).IsAssignableFrom(objectType), $"{nameof(objectType)} must be of type {typeof(UnityEngine.Object)}");
		Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} must be an interface type");
		ObjectType = objectType;
		InterfaceType = interfaceType;
	}
}