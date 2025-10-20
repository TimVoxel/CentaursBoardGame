using System;
using UnityEngine;

#nullable enable

[Serializable]
public class InterfaceReference<TI, TO> where TO : UnityEngine.Object where TI : class
{
    [SerializeField, HideInInspector] private TO? _value;

    public TI? Value
    {
        get
        {
            if (_value == null)
            {
                return null;
            }
            else if (_value is TI @interface)
            {
                return @interface;
            }
            else
            {
                throw new InvalidOperationException($"{_value} must implement interface {nameof(TI)}");
            }
        }
        set
        {
            if (value == null)
            {
                _value = null;
            }
            else if (value is TO newValue)
            {
                _value = newValue;
            }
            else
            {
                throw new ArgumentException($"{_value} must be of type {typeof(TO)}.");
            }
        }
    }

    public TO? UnderlyingValue
    {
        get => _value;
        set => _value = value;
    }

    public InterfaceReference() { }

    public InterfaceReference(TO? value)
    {
        _value = value;
    }

    public InterfaceReference(TI @interface)
    {
        _value = @interface as TO;
    }

}

[Serializable]
public class InterfaceReference<TI> : InterfaceReference<TI, UnityEngine.Object> where TI : class { }
