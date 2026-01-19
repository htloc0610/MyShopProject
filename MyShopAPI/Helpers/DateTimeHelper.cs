using System;

namespace MyShopAPI.Helpers;

/// <summary>
/// Helper class for consistent timezone handling across the application.
/// Uses Vietnam timezone (UTC+7) for all date/time operations.
/// </summary>
public static class DateTimeHelper
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    
    /// <summary>
    /// Gets the current date and time in Vietnam timezone (UTC+7).
    /// Use this instead of DateTime.UtcNow or DateTime.Now.
    /// </summary>
    public static DateTime Now => DateTime.UtcNow.Add(VietnamOffset);
    
    /// <summary>
    /// Gets today's date in Vietnam timezone (UTC+7).
    /// </summary>
    public static DateTime Today => Now.Date;
    
    /// <summary>
    /// Converts a Vietnam local time to UTC for database storage.
    /// </summary>
    public static DateTime ToUtc(DateTime vietnamTime)
    {
        var utcTime = vietnamTime.Subtract(VietnamOffset);
        return DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
    }
    
    /// <summary>
    /// Converts a UTC time from database to Vietnam local time for display.
    /// </summary>
    public static DateTime ToVietnam(DateTime utcTime)
    {
        return utcTime.Add(VietnamOffset);
    }
    
    /// <summary>
    /// Converts nullable UTC time to Vietnam local time.
    /// </summary>
    public static DateTime? ToVietnam(DateTime? utcTime)
    {
        return utcTime?.Add(VietnamOffset);
    }
}
