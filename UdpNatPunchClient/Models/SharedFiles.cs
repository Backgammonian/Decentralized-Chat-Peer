using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace UdpNatPunchClient.Models
{
    public sealed class SharedFiles
    {
        private readonly ConcurrentDictionary<string, SharedFile> _files;

        public SharedFiles()
        {
            _files = new ConcurrentDictionary<string, SharedFile>();
        }

        public event EventHandler<EventArgs>? SharedFileAdded;
        public event EventHandler<EventArgs>? SharedFileHashCalculated;
        public event EventHandler<SharedFileEventArgs>? SharedFileError;
        public event EventHandler<SharedFileEventArgs>? SharedFileRemoved;

        public IEnumerable<SharedFile> SharedFilesList => _files.Values;

        public SharedFile? GetByID(string fileID)
        {
            try
            {
                return _files.Values.First(sharedFile =>
                    sharedFile.ID == fileID &&
                    sharedFile.IsHashCalculated);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<SharedFileInfo> GetAvailableFiles()
        {
            return _files.Values
                .Where(sharedFile => sharedFile.IsHashCalculated)
                .Select(sharedFile => new SharedFileInfo(sharedFile))
                .ToList();
        }

        public async Task<SharedFile?> AddFile(string filePath)
        {
            return await Task.Run(() => AddFileRoutine(filePath));
        }

        private SharedFile? AddFileRoutine(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"(AddFileRoutine) File {filePath} doesn't exist");

                return null;
            }

            var sharedFile = new SharedFile(filePath);

            if (sharedFile.TryOpenStream() &&
                _files.TryAdd(sharedFile.ID, sharedFile))
            {
                SharedFileAdded?.Invoke(this, EventArgs.Empty);

                if (sharedFile.TryComputeFileHash())
                {
                    SharedFileHashCalculated?.Invoke(this, EventArgs.Empty);

                    return sharedFile;
                }
                else
                {
                    RemoveFile(sharedFile.ID);

                    return null;
                }
            }
            else
            {
                RemoveFile(sharedFile.ID);
                SharedFileError?.Invoke(this, new SharedFileEventArgs(sharedFile));

                return null;
            }
        }

        public void RemoveFile(string fileID)
        {
            if (_files.TryRemove(fileID, out SharedFile? removedFile) &&
                removedFile != null)
            {
                removedFile.CloseStream();
                SharedFileRemoved?.Invoke(this, new SharedFileEventArgs(removedFile));
            }
        }

        public void CloseAllFileStreams()
        {
            foreach (var sharedFile in _files.Values)
            {
                sharedFile.CloseStream();
            }
        }
    }
}
