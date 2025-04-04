namespace WebApp.Helpers;

public static class ProjectHelper
{
    public static string? GetTimeLeft(DateTime? endDate)
    {
        if (endDate == null)
            return null;

        var remaining = endDate.Value.Date - DateTime.Today;

        if (remaining.TotalDays <= 0)
            return "Ended";

        if (remaining.TotalDays <= 7)
            return $"{remaining.Days} day{(remaining.Days == 1 ? "" : "s")} left";

        var weeks = (int)Math.Ceiling(remaining.TotalDays / 7);
        return $"{weeks} week{(weeks == 1 ? "" : "s")} left";
    }

    public static bool IsCompleted(DateTime? endDate)
    {
        return endDate.HasValue && endDate.Value.Date < DateTime.Today;
    }

    public static bool IsEndingSoon(DateTime? endDate, int thresholdDays = 7)
    {
        return endDate.HasValue && (endDate.Value - DateTime.Today).TotalDays <= thresholdDays;
    }
}
