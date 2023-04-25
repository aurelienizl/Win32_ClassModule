using System.Diagnostics;


public class WrsLogs
{
    private static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static string WrsDirectory = Path.Combine(AppDataPath, "wrs");
    private static string LogFilePath = Path.Combine(WrsDirectory, "log.txt");
    private EventLog? eventLog;
    private Mutex mutex;

    public WrsLogs()
    {
        InitializeLogs();
        mutex = new Mutex();
    }

    private void InitializeLogs()
    {
        if (!Directory.Exists(WrsDirectory))
        {
            Directory.CreateDirectory(WrsDirectory);
        }

        if (!File.Exists(LogFilePath))
        {
            File.Create(LogFilePath).Dispose();
        }

        CheckAndRemoveOldLogFile();

        eventLog = new EventLog();
        if (!EventLog.SourceExists("WrsLogs"))
        {
            EventLog.CreateEventSource("WrsLogs", "Application");
        }
        eventLog.Source = "WrsLogs";
    }

    private void CheckAndRemoveOldLogFile()
    {
        if (File.Exists(LogFilePath))
        {
            FileInfo fileInfo = new FileInfo(LogFilePath);
            long fileSizeInBytes = fileInfo.Length;
            long fileSizeInMegabytes = fileSizeInBytes / (1024 * 1024);

            if (fileSizeInMegabytes >= 100)
            {
                File.Delete(LogFilePath);
            }
        }
    }

    public void LogInfo(string message)
    {
        System.Console.WriteLine(message);
        WriteToFile("INFO", message);
        WriteToEventLog(EventLogEntryType.Information, message);
    }

    public void LogError(string message)
    {
        System.Console.Error.WriteLine(message);
        WriteToFile("ERROR", message);
        WriteToEventLog(EventLogEntryType.Error, message);
    }

    public void LogCritical(string message)
    {
        System.Console.Error.WriteLine(message);
        WriteToFile("CRITICAL", message);
        WriteToEventLog(EventLogEntryType.Error, message);
    }

    private void WriteToFile(string logType, string message)
    {
        mutex.WaitOne();
        try
        {
            using (StreamWriter sw = new StreamWriter(LogFilePath, true))
            {
                sw.WriteLine($"{DateTime.Now} - {logType}: {message}");
            }
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private void WriteToEventLog(EventLogEntryType entryType, string message)
    {
        mutex.WaitOne();
        try
        {
            eventLog!.WriteEntry(message, entryType);
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
}
