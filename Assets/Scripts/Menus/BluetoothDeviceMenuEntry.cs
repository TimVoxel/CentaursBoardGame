using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
public class BluetoothDeviceMenuEntry : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _deviceNameText;
	[SerializeField] private Button _button;

	private BluetoothDeviceInfo? _deviceInfo;

	public event Action<BluetoothDeviceMenuEntry>? OnConnectRequested;

	public BluetoothDeviceInfo DeviceInfo
	{
		get => _deviceInfo ?? throw new InvalidOperationException("DeviceInfo is not set.");
		set
		{
			_deviceInfo = value;
			_deviceNameText.text = value?.Name ?? "<unbound>";
			_button.enabled = value != null;
        }
    }

	public void Bind(BluetoothDeviceInfo deviceInfo)
	{
		DeviceInfo = deviceInfo;
	}

	public void TryConnect()
	{
		OnConnectRequested?.Invoke(this);
    }
}
