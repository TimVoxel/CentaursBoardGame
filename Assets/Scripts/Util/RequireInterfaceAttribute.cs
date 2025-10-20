using System;
using UnityEngine;

#nullable enable

[AttributeUsage(AttributeTargets.Field)]
public class RequireInterfaceAttribute : PropertyAttribute
{
	public Type InterfaceType { get; }

	public RequireInterfaceAttribute(Type interfaceType)
    {
        Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} must be an interface type");
        InterfaceType = interfaceType;
    }
}