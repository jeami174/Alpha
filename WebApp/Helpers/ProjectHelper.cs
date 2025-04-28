namespace WebApp.Helpers
{
    public static class ProjectHelper
    {
        public static string GetTimeLeft(DateTime startDate, DateTime? endDate)
        {
            var today = DateTime.Today;

            if (!endDate.HasValue)
                return "";

            if (today < startDate)
                return "Not started";

            if (today > endDate.Value)
                return "Ended";

            var daysRemaining = (endDate.Value.Date - today).Days;

            if (daysRemaining <= 7)
                return $"{daysRemaining} day{(daysRemaining == 1 ? "" : "s")} left";

            var weeks = (int)Math.Ceiling(daysRemaining / 7.0);
            return $"{weeks} week{(weeks == 1 ? "" : "s")} left";
        }

        public static bool IsNotStarted(DateTime startDate)
            => DateTime.Today < startDate;

        public static bool IsCompleted(DateTime? endDate)
            => endDate.HasValue && DateTime.Today > endDate.Value;

        public static bool IsEndingSoon(DateTime? endDate, int thresholdDays = 7)
        {
            if (!endDate.HasValue)
                return false;

            var daysLeft = (endDate.Value.Date - DateTime.Today).TotalDays;
            return daysLeft >= 0 && daysLeft <= thresholdDays;
        }
    }
}
