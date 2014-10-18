#if DEBUG
using System;
using System.Threading;

public class DebugEx
{
    public static void WriteLine(string format, params object[] args)
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            return;

        //string threadId = string.IsNullOrEmpty(Thread.CurrentThread.Name) ? Thread.CurrentThread.ManagedThreadId.ToString() : Thread.CurrentThread.Name;
        //string timestamp = string.Format("THREAD: {0}, AT: {1}", threadId, DateTime.Now.ToString("hh:mm:ss.fff tt"));
        //System.Diagnostics.Debug.WriteLine(timestamp + "\t" + format, args);

        System.Diagnostics.Debug.WriteLine(format, args);
    }

    public static void WriteLine(object o)
    {
        if (o != null)
            WriteLine(o.ToString());
    }
}

#else

public class DebugEx
{
    public static void WriteLine(string format, params object[] args) {}
    public static void WriteLine(object o) {}
}

#endif
