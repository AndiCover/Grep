using System;
using System.IO;

namespace Grep
{
    /// <summary>
    /// Synchronous Grep option. Very Slow but everything is ordered. Only here to compare performance and cpu/memory usage.
    /// </summary>
    class GrepSynchronous : GrepBase
    {
        public GrepSynchronous(Boolean showExceptions, Boolean showThread, int lineSize)
            : base(showExceptions, showThread, lineSize)
        {
            //Nothing to do.
        }

        /// <summary>
        /// Process directories and files synchronously.
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
                    PrintExceptionToConsole(ex);
                    //Input is propably a file.
                    files = new string[] { directory };
                }

                foreach (string filename in files)
                {
                    ProcessFile(filename, regex);
                }
                foreach (string directoryName in directories)
                {
                    ProcessDirectory(directoryName, regex);
                }
            }
        }
    }
}