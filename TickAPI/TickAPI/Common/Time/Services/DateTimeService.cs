using TickAPI.Common.Time.Abstractions;

namespace TickAPI.Common.Time.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime GetCurrentDateTime()
    {
        return DateTime.UtcNow;
    }
}