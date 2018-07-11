// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

namespace MasterServerKit
{
    /// <summary>
    /// Miscellaneous utilities to convert between DateTime and Unix time.
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// Converts the specified DateTime to a Unix time.
        /// </summary>
        /// <param name="dateTime">DateTime to convert to Unix time.</param>
        /// <returns>The converted Unix time.</returns>
        public static double DateTimeToUnixTime(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Converts the specified Unix time to a DateTime.
        /// </summary>
        /// <param name="unixTimeStamp">Unix time to convert to a DateTime.</param>
        /// <returns>The converted DateTime.</returns>
        public static DateTime UnixTimeToDateTime(double unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
