namespace AttendanceManagement.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <summary>
        /// Convert UTC time to Vietnam timezone (UTC+7)
        /// </summary>
        public static DateTime ToVietnamTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTime(utcTime, VietnamTimeZone);
        }

        /// <summary>
        /// Get current time in Vietnam timezone
        /// </summary>
        public static DateTime GetVietnamNow()
        {
            return ToVietnamTime(DateTime.UtcNow);
        }
    }
}
