using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Grep
{
    /// <summary>
    /// Abstract class for the Grep operations.
    /// </summary>
    public abstract class GrepBase
    {
        private Boolean showExceptions = false;
        private Boolean showThread = true;
        private int lineSize = 50;
        private int occurrences = 0;
        private ConcurrentDictionary<int, DateTime> threads = new ConcurrentDictionary<int, DateTime>();

        public GrepBase()
        {
            //Nothing to do.
        }

        public GrepBase(Boolean showExceptions, Boolean showThread, int lineSize)
        {
            this.showExceptions = showExceptions;
            this.showThread = showThread;
            this.lineSize = lineSize;
        }

        public Boolean ShowThread()
        {
            return this.showThread;
        }

        public int GetOccurences()
        {
            return this.occurrences;
        }

        public ConcurrentDictionary<int, DateTime> Threads()
        {
            return this.threads;
        }

        /// <summary>
        /// Prints the formatted filename and a parts of the line to the console.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="line"></param>
        /// <param name="regex"></param>
        /// <param name="matches"></param>
        protected void PrintFileToConsole(string filename, string line, MatchCollection matches)
        {
            if (!String.IsNullOrEmpty(filename) && !String.IsNullOrEmpty(line) && matches != null)
            {
                foreach (Match match in matches)
                {
                    //in parallel mode this method gets locked anyway so no need to lock the counter. But this is an atomic operation anyway.
                    occurrences++;

                    int index = match.Index;
                    PrintHighlightedFilename(filename + ": ");

                    int startIndex = index - lineSize;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    int stopLength = lineSize;
                    if (startIndex + stopLength > line.Length)
                    {
                        stopLength = line.Length - startIndex;
                    }

                    Console.Write(line.Substring(startIndex, stopLength));
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(line.Substring(index, match.Length));
                    Console.ResetColor();

                    if (index + match.Length + stopLength > line.Length)
                    {
                        stopLength = line.Length - index - match.Length;
                    }
                    startIndex = index + match.Length;
                    Console.WriteLine(line.Substring(startIndex, stopLength));
                }
            }
        }

        /// <summary>
        /// Prints the formatted filename to the console.
        /// </summary>
        /// <param name="filename">The filename</param>
        protected void PrintHighlightedFilename(string filename)
        {
            if (!String.IsNullOrEmpty(filename))
            {
                if (showThread)
                {
                    Console.Write(String.Format("[{0}]: ", Thread.CurrentThread.ManagedThreadId));
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(filename);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Prints the given exception to the console if SHOW_EXCEPTIONS is active.
        /// </summary>
        /// <param name="ex">The exception</param>
        protected void PrintExceptionToConsole(Exception ex)
        {
            if (ex != null && showExceptions)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Executes the grep option.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="regex"></param>
        public void PrintPath(string path, string regex)
        {
            ProcessDirectory(path, regex);
        }

        /// <summary>
        /// Method to remove some special chars. I.e. the beep character.
        /// </summary>
        /// <param name="line">line</param>
        /// <returns>prepared line</returns>
        protected string PrepareLine(string line)
        {
            if (!String.IsNullOrEmpty(line))
            {
                //I was wondering why my computer started randomly to beep.
                //Funny thing: I need to escape the bell character '\u0007'.
                return line.Replace("\u0007", "\\u0007");
            }
            return "";
        }

        /// <summary>
        /// Method to process a directory.
        /// </summary>
        /// <param name="directory">path</param>
        /// <param name="regex"></param>
        protected abstract void ProcessDirectory(string directory, string regex);

        /// <summary>
        /// Processes a single file. Override if neccessary.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="regex"></param>
        protected virtual void ProcessFile(string filename, string regex)
        {
            if (!String.IsNullOrEmpty(filename) && !String.IsNullOrEmpty(regex))
            {
                try
                {
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
                    PrintExceptionToConsole(ex);
                }
            }
        }
    }
}