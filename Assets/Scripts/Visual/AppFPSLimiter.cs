using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    public static class AppFPSLimiter
    {
        public static void Run()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
    }
}