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
        private readonly ConcurrentDictionary<long, SharedFile> _files;
        private readonly Indexer _indexer;

        public SharedFiles()
        {
            _files = new ConcurrentDictionary<long, SharedFile>();
            _indexer = new Indexer();
        }

        public event EventHandler<EventArgs>? SharedFileAdded;
        public event EventHandler<EventArgs>? SharedFileHashCalculated;
        public event EventHandler<SharedFileEventArgs>? SharedFileError;
        public event EventHandler<EventArgs>? SharedFileRemoved;

        public IEnumerable<SharedFile> SharedFilesList => _files.Values;

        public bool HasFile(long index)
        {
            return _files.ContainsKey(index);
        }

        public SharedFile? GetByHash(string fileHash)
        {
            try
            {
                return _files.Values.First(sharedFile =>
                    sharedFile.Hash == fileHash && sharedFile.IsHashCalculated);
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

        public void AddFile(string filePath)
        {
            var addFileTask = new Task(() =>
            {
                AddFileRoutine(filePath);
            });
            addFileTask.Start();
        }

        private void AddFileRoutine(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"(AddFileRoutine) File {filePath} doesn't exist");

                return;
            }

            var index = _indexer.GetNewIndex();
            var sharedFile = new SharedFile(index, filePath);

            if (sharedFile.TryOpenStream() &&
                _files.TryAdd(sharedFile.Index, sharedFile))
            {
                SharedFileAdded?.Invoke(this, EventArgs.Empty);

                if (_files[sharedFile.Index].TryComputeFileHash())
                {
                    SharedFileHashCalculated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    RemoveFile(sharedFile.Index);
                }
            }
            else
            {
                RemoveFile(index);
                SharedFileError?.Invoke(this, new SharedFileEventArgs(filePath));
            }
        }

        public void RemoveFile(long fileIndex)
        {
            if (HasFile(fileIndex) &&
                _files.TryRemove(fileIndex, out SharedFile? removedFile) &&
                removedFile != null)
            {
                removedFile.CloseStream();
                SharedFileRemoved?.Invoke(this, EventArgs.Empty);
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
