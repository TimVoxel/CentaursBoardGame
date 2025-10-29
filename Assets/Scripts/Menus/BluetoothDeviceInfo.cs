using System;
using UnityEngine;
#nullable enable

[Serializable]
public class BluetoothDeviceInfo
{
	[SerializeField] private string _name;
	[SerializeField] private string _address;

    public string Name => _name;
	public string Address => _address;

	public BluetoothDeviceInfo(string name, string address)
	{
		_name = name;
		_address = address;
    }
}
