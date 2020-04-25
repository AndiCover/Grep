using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace Grep
{
    /// <summary>
    /// Parallel Grep option. Uses the Task Parallel Library. Fast but there is no guaranteed order.
    /// </summary>
    class GrepParallel : GrepBase
    {
        private static object _MessageLock = new object();

        public GrepParallel(Boolean showExceptions, Boolean showThread, int lineSize)
            : base(showExceptions, showThread, lineSize)
        {
            //Nothing to do.
        }

        /// <summary>
        /// Hide base#PrintFileToConsole and lock the console.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="line"></param>
        /// <param name="regex"></param>
        /// <param name="matches"></param>
        protected new void PrintFileToConsole(string filename, string line, MatchCollection matches)
        {
            //Locking of console to ensure that the output is not mixed.
            lock (_MessageLock)
            {
                base.PrintFileToConsole(filename, line, matches);
            }
        }

        /// <summary>
        /// Process directories and files parallel.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="regex"></param>
        protected override void ProcessDirectory(string directory, string regex)
        {
            if (!String.IsNullOrEmpty(directory) && !String.IsNullOrEmpty(regex))
            {
                string[] files = new string[0];
                string[] directories = new string[0];

                try
                {
                    files = Directory.GetFiles(directory);
                    directories = Directory.GetDirectories(directory);
                }
                catch (Exception ex)
                {
                    //Locking of console to ensure that the output is not mixed.
                    lock (_MessageLock)
                    {
                        PrintExceptionToConsole(ex);
                    }
                    //Input is propably a file.
                    files = new string[] { directory };
                }

                //I do not care about the processing order of files. So parallel is ok.
                Parallel.ForEach(files, (filename) =>
                {
                    if (ShowThread())
                    {
                        int threadId = Thread.CurrentThread.ManagedThreadId;
                        if (!Threads().ContainsKey(threadId))
                        {
                            try
                            {
                                Threads().TryAdd(threadId, DateTime.Now);
                            }
                            catch (Exception ex)
                            {
                                PrintExceptionToConsole(ex);
                            }
                        }
                    }
                    ProcessFile(filename, regex);
                });
                //I do not care about the processing order of directories. So parallel is ok.
                Parallel.ForEach(directories, (directoryName) =>
                {
                    ProcessDirectory(directoryName, regex);
                });
            }
        }

        protected override void ProcessFile(string filename, string regex)
        {
            if (!String.IsNullOrEmpty(filename) && !String.IsNullOrEmpty(regex))
            {
                try
                {
                    //Another Parallel.ForEach would lead to too much threads and that would have a negative impact on the performance. Lines will be processed synchronously.
                    foreach (string line in File.ReadLines(filename))
                    {
                        string preparedLine = PrepareLine(line);
                        MatchCollection matches = Regex.Matches(preparedLine, regex);
                        if (matches.Count > 0)
                        {
                            PrintFileToConsole(filename, preparedLine, matches);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Locking of console to ensure that the output is not mixed.
                    lock (_MessageLock)
                    {
                        PrintExceptionToConsole(ex);
                    }
                }
            }
        }
    }
}