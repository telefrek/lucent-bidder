namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public enum VolumeNormalizationMode
    {
        /// <value></value>
        None = 0,
        /// <value></value>
        AdVolumeAverage = 1,
        /// <value></value>
        AdVolumePeak = 2,
        /// <value></value>
        AdLoudness = 3,
        /// <value></value>
        CustomNormalization = 4,
    }
}