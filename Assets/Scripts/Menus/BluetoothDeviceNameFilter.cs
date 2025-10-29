using System;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    [CreateAssetMenu(fileName = "NameBluetoothDeviceFilter", menuName = "CentaursBoardGame/Bluetooth Device Name Filter")]
    public class BluetoothDeviceNameFilter : BluetoothDeviceFilter
    {
        [SerializeField] private string _name;
        [SerializeField] private StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

        public override bool Matches(BluetoothDeviceInfo info)
            => info.Name.Equals(_name, _stringComparison);
    }
}