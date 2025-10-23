using System;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    [Serializable]
    public struct PeripheralData
    {
        [SerializeField] private string _serviceUUID;
        [SerializeField] private string _characteristicUUID;
        [SerializeField] private string _name;

        public string ServiceUUID => _serviceUUID;
        public string CharacteristicUUID => _characteristicUUID;
        public string Name => _name;

        public PeripheralData(string serviceUUID, string characteristicUUID, string name)
        {
            _serviceUUID = serviceUUID;
            _characteristicUUID = characteristicUUID;
            _name = name;
        }
    }

}