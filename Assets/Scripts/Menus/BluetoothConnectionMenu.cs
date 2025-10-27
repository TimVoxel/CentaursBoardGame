using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable enable

namespace CentaursBoardGame
{
	public class BluetoothConnectionMenu : MonoBehaviour
	{
		[SerializeField] private BLECommunicator _handle;
		[SerializeField] private BluetoothDeviceMenuEntry _prefab;
        [SerializeField] private GridLayoutGroup _layoutGroup;

        [SerializeField] private UnityEvent<BluetoothDeviceInfo>? _onConnectRequested;
        [SerializeField] private UnityEvent? _onShow;
        [SerializeField] private UnityEvent? _onHide;

        private RectTransform? _layoutGroupTransform;
        private List<BluetoothDeviceMenuEntry> _entries = new List<BluetoothDeviceMenuEntry>();
        private bool _isShown;

        private void Awake()
        {
            _layoutGroupTransform = _layoutGroup.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _handle.OnFoundDevice += AddEntry;
            _handle.OnDisconnected += _handle.StartScan;

            foreach (var entry in _entries)
            {
                entry.OnConnectRequested += TryConnectToDevice;
            }
        }

        private void OnDisable()
        {
            _handle.OnFoundDevice -= AddEntry;
            _handle.OnDisconnected -= _handle.StartScan;

            foreach (var entry in _entries)
            {
                entry.OnConnectRequested -= TryConnectToDevice;
            }
        }

        private void AddEntry(BluetoothDeviceInfo info)
        {
            var sizePerCell = _layoutGroup.cellSize.y + _layoutGroup.spacing.y;

            _layoutGroup.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                _layoutGroup.padding.top + sizePerCell * _entries.Count);

            var entry = Instantiate(_prefab, _layoutGroupTransform);
            entry.Bind(info);
            entry.OnConnectRequested += TryConnectToDevice;
            _entries.Add(entry);
        }
        
        private void DeleteEntry(BluetoothDeviceInfo deviceInfo)
        {
            var entry = _entries.FirstOrDefault(e => e.DeviceInfo == deviceInfo);

            if (entry == null)
            {
                throw new Exception("Trying to delete null entry");
            }

            entry.OnConnectRequested -= TryConnectToDevice;
            _entries.Remove(entry);
            Destroy(entry.gameObject);
        }

        private void ClearEntries()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                entry.OnConnectRequested -= TryConnectToDevice;
                Destroy(entry.gameObject);
                _entries.RemoveAt(i);
            }
        }

        public void Show()
        {
            _isShown = true;
            _onShow?.Invoke();

            ClearEntries();
            _handle.StartScan();
        }

        public void Hide()
        {
            _isShown = false;
            _onHide?.Invoke();
        }

        public void Toggle()
        {
            if (!_isShown)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void TryConnectToDevice(BluetoothDeviceInfo deviceInfo)
        {
            _handle.TryConnect(deviceInfo.Address);
            _onConnectRequested?.Invoke(deviceInfo);
        }
    }
}