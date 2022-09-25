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

        public IEnumerable<AvailableFile> AvailableFilesList => _files.Values;

        public AvailableFile? GetByHash(string fileHash)
        {
            try
            {
                return _files.Values.First(sharedFile =>
                    sharedFile.Hash == fileHash);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Add(AvailableFile availableFile)
        {
            if (_files.TryAdd(availableFile.ID, availableFile))
            {
                FileAdded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void MarkAllFilesFromServerAsUnavailable(int serverID)
        {
            var filesFromServer = _files.Values.Where(file => file.Server.PeerID == serverID);

            foreach (var file in filesFromServer)
            {
                file.MarkAsUnavailable();
            }
        }
    }
}
