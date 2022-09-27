using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UdpNatPunchClient.Models
{
    public sealed class AvailableFiles
    {
        private readonly ConcurrentDictionary<string, AvailableFile> _files;

        public AvailableFiles()
        {
            _files = new ConcurrentDictionary<string, AvailableFile>();
        }

        public event EventHandler<EventArgs>? FileAdded;
        public event EventHandler<EventArgs>? FileRemoved;

        public IEnumerable<AvailableFile> AvailableFilesList => _files.Values;

        public AvailableFile? GetByFileID(string fileID)
        {
            try
            {
                return _files.Values.First(availableFile =>
                    availableFile.FileIDFromServer == fileID);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Add(AvailableFile availableFile)
        {
            if (_files.TryAdd(availableFile.FileIDFromServer, availableFile))
            {
                FileAdded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Remove(string fileID)
        {
            if (_files.ContainsKey(fileID) &&
                _files.TryRemove(fileID, out var removedFile))
            {
                removedFile.MarkAsUnavailable();
                FileRemoved?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveAllFilesFromServer(int serverID)
        {
            var filesFromServer = _files.Values.Where(file => file.Server.PeerID == serverID);

            foreach (var file in filesFromServer)
            {
                Remove(file.FileIDFromServer);
            }
        }
    }
}
