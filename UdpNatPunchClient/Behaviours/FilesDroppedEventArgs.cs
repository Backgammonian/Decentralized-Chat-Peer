using System;

namespace DropFiles
{
    public sealed class FilesDroppedEventArgs : EventArgs
    {
        public FilesDroppedEventArgs(string[] filesPaths)
        {
            FilesPath = filesPaths;
        }

        public string[] FilesPath { get; }
    }
}
