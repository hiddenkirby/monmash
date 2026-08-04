using System;

namespace Tidepool.Domain
{
    [Serializable]
    public class SettingsData
    {
        public int schemaVersion = 1;
        public bool muted;
        public float masterVolume = 1f;
    }
}
