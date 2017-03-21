using System;

namespace PixonicEcho
{
    static class MyConsole
    {
        public static void WriteLine(string format, params object[] args)
        {
            if (Settings.PrintMessagesToConsole)
                Console.WriteLine(format, args);
        }

        public static void WriteLine(object obj)
        {
            if (Settings.PrintMessagesToConsole)
                Console.WriteLine(obj);
        }
    }
}