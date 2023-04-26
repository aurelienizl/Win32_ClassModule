using System.Net;
using System.Collections.Concurrent;

public class IPBlacklist
{
    private class IPEntry
    {
        public int FailedAttempts { get; set; }
        public DateTime LastFailedAttempt { get; set; }
        public DateTime? BlockedUntil { get; set; }
    }

    private readonly ConcurrentDictionary<IPAddress, IPEntry> _ipEntries = new ConcurrentDictionary<IPAddress, IPEntry>();
    private readonly TimeSpan _blockDuration = TimeSpan.FromDays(1);
    private readonly TimeSpan _attemptInterval = TimeSpan.FromMinutes(30);
    private readonly int _maxFailedAttempts = 5;

    public bool IsAllowed(IPAddress ipAddress)
    {
        if (_ipEntries.TryGetValue(ipAddress, out IPEntry? entry))
        {
            if (entry.BlockedUntil.HasValue && DateTime.UtcNow < entry.BlockedUntil.Value)
            {
                return false;
            }
        }

        return true;
    }

    public void RegisterFailedAttempt(IPAddress ipAddress)
    {
        IPEntry entry = _ipEntries.GetOrAdd(ipAddress, new IPEntry());
        DateTime now = DateTime.UtcNow;

        if (now - entry.LastFailedAttempt > _attemptInterval)
        {
            entry.FailedAttempts = 1;
        }
        else
        {
            entry.FailedAttempts++;
        }

        entry.LastFailedAttempt = now;

        if (entry.FailedAttempts >= _maxFailedAttempts)
        {
            entry.BlockedUntil = now + _blockDuration;
        }
    }
}