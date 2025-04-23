namespace WebApp.Helpers
{
    public static class ProjectHelper
    {
        public static string GetTimeLeft(DateTime endDate)
        {
            var today = DateTime.Today;
            var daysRemaining = (endDate.Date - today).Days;

            if (daysRemaining <= 0)
                return "Ended";

            if (daysRemaining <= 7)
                return $"{daysRemaining} day{(daysRemaining == 1 ? "" : "s")} left";

            var weeks = (int)Math.Ceiling((endDate.Date - today).TotalDays / 7);
            return $"{weeks} week{(weeks == 1 ? "" : "s")} left";
        }

        public static bool IsNotStarted(DateTime startDate)
            => startDate > DateTime.Today;

        public static bool IsCompleted(DateTime? endDate)
            => endDate.HasValue && endDate.Value.Date < DateTime.Today;

        public static bool IsEndingSoon(DateTime? endDate, int thresholdDays = 7)
        {
            if (!endDate.HasValue)
                return false;

            var daysLeft = (endDate.Value.Date - DateTime.Today).TotalDays;
            return daysLeft >= 0 && daysLeft <= thresholdDays;
        }
    }
}
