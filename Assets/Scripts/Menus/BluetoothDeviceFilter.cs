using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    public abstract class BluetoothDeviceFilter : ScriptableObject
    {
        public abstract bool Matches(BluetoothDeviceInfo info);
    }
}