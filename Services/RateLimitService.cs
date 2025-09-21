using System.Collections.Concurrent;

namespace SimpleCRM.Services
{
    public interface IRateLimitService
    {
        bool IsRequestAllowed(string key, int maxAttempts = 5, TimeSpan? timeWindow = null);
        void ResetAttempts(string key);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly ConcurrentDictionary<string, (DateTime lastAttempt, int attempts)> _attempts = new();
        private readonly TimeSpan _defaultTimeWindow = TimeSpan.FromMinutes(15);

        public bool IsRequestAllowed(string key, int maxAttempts = 5, TimeSpan? timeWindow = null)
        {
            var window = timeWindow ?? _defaultTimeWindow;
            var now = DateTime.Now;

            _attempts.AddOrUpdate(key,
                (now, 1),
                (k, existing) =>
                {
                    // Reset if outside time window
                    if (now - existing.lastAttempt > window)
                    {
                        return (now, 1);
                    }
                    return (now, existing.attempts + 1);
                });

            var current = _attempts[key];
            return current.attempts <= maxAttempts;
        }

        public void ResetAttempts(string key)
        {
            _attempts.TryRemove(key, out _);
        }
    }
}
