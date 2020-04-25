using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Grep
{
    /// <summary>
    /// Grep class to list all files that contain a given string.
    /// </summary>
    /// <remarks>
    /// Following parameters are required to call grep properly:
    /// 1: grep option (-s ... Synchronous, -p ... Parallel)
    /// 2: the regular expression
    /// 3: root path for this operation
    /// </remarks>
    class Grep
    {
        private static readonly Boolean SHOW_EXCEPTIONS = false;
        private static readonly Boolean SHOW_THREAD = true;
        private static readonly int LINE_SIZE = 50;

        static void Main(string[] args)
        {
            GrepBase grep = null;
            if (args.Length == 3)
            {
                string grepOption = args[0];
                string regex = args[1];
                string path = args[2];

                switch (grepOption.ToUpper())
                {
                    case "-S":
                        grep = new GrepSynchronous(SHOW_EXCEPTIONS, SHOW_THREAD, LINE_SIZE);
                        break;
                    case "-P":
                        grep = new GrepParallel(SHOW_EXCEPTIONS, SHOW_THREAD, LINE_SIZE);
                        break;
                    default:
                        Console.WriteLine(String.Format("Invalid program argument: '{0}'", grepOption));
                        break;
                }
                if (grep != null)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    DateTime start = DateTime.Now;
                    grep.PrintPath(path, regex);
                    stopwatch.Stop();
                    TimeSpan synchronousDuration = stopwatch.Elapsed;
                    Console.WriteLine(String.Format("\n\nOccurences: {0}\nDuration: {1}\n\n", grep.GetOccurences(), synchronousDuration));

                    if (grep.ShowThread())
                    {
                        foreach (KeyValuePair<int, DateTime> threadValue in grep.Threads())
                        {
                            Console.WriteLine(String.Format("Thread {0} created {1} after start", threadValue.Key, threadValue.Value - start));
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Wrong number of input parameter!");
            }
            Console.ReadKey();
        }
    }
}