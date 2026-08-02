namespace Tidepool.Domain
{
    public static class TidelingLevelProgression
    {
        public const int ProgressPerLevel = 3;

        public static void Normalize(CaughtTideling caught)
        {
            if (caught == null)
            {
                return;
            }

            caught.level = ClampLevel(caught.level);
            caught.levelProgress = caught.level >= CaughtTideling.MaxLevel
                ? 0
                : ClampProgress(caught.levelProgress);
        }

        public static bool AddProgress(CaughtTideling caught, int amount)
        {
            if (caught == null || amount <= 0 || caught.level >= CaughtTideling.MaxLevel)
            {
                return false;
            }

            int previousLevel = caught.level;
            int previousProgress = caught.levelProgress;
            caught.levelProgress += amount;

            while (caught.levelProgress >= ProgressPerLevel && caught.level < CaughtTideling.MaxLevel)
            {
                caught.level += 1;
                caught.levelProgress -= ProgressPerLevel;
            }

            if (caught.level >= CaughtTideling.MaxLevel)
            {
                caught.level = CaughtTideling.MaxLevel;
                caught.levelProgress = 0;
            }

            return caught.level != previousLevel || caught.levelProgress != previousProgress;
        }

        private static int ClampLevel(int level)
        {
            if (level < CaughtTideling.MinLevel)
            {
                return CaughtTideling.MinLevel;
            }

            return level > CaughtTideling.MaxLevel ? CaughtTideling.MaxLevel : level;
        }

        private static int ClampProgress(int progress)
        {
            if (progress < 0)
            {
                return 0;
            }

            return progress >= ProgressPerLevel ? ProgressPerLevel - 1 : progress;
        }
    }
}
