namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents the quality of a GNSS (GPS) position fix.
    /// 
    /// This describes how the latitude/longitude was computed and therefore
    /// how reliable and precise the reported location is.
    /// 
    /// It is commonly derived from GNSS receiver metadata and should be used
    /// by routing, matching, and analytics systems to decide whether a point
    /// can be trusted.
    /// </summary>
    public enum FixQuality
    {
        /// <summary>
        /// The fix quality is unknown or not provided.
        /// This should be treated as low confidence.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A standard GNSS fix using only satellite data.
        /// Typical consumer GPS accuracy (3–10 meters).
        /// </summary>
        Autonomous = 1,

        /// <summary>
        /// A differential GNSS fix using correction data (e.g., SBAS, DGPS).
        /// Provides improved accuracy over autonomous fixes.
        /// </summary>
        Differential = 2,

        /// <summary>
        /// A Real-Time Kinematic (RTK) fixed solution.
        /// High-precision, centimeter-level accuracy.
        /// Suitable for lane-level or asset-level tracking.
        /// </summary>
        RtkFixed = 3,

        /// <summary>
        /// A Real-Time Kinematic (RTK) float solution.
        /// More accurate than differential GNSS but not as precise as RTK fixed.
        /// Typically decimeter-level accuracy.
        /// </summary>
        RtkFloat = 4
    }
}