using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace UdpNatPunchClient.Models
{
    public sealed class Downloads
    {
        private readonly ConcurrentDictionary<string, Download> _downloads;

        public Downloads()
        {
            _downloads = new ConcurrentDictionary<string, Download>();
        }

        public event EventHandler<EventArgs>? DownloadsListUpdated;
        public event EventHandler<DownloadFinishedEventArgs>? DownloadFinished;

        public IEnumerable<Download> DownloadsList =>
            _downloads.Values.OrderBy(download => download.StartTime);

        private void OnFileRemoved(object? sender, EventArgs e)
        {
            DownloadsListUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnDownloadFinished(object? sender, DownloadFinishedEventArgs e)
        {
            DownloadFinished?.Invoke(this, e);
        }

        public Download? GetDownloadWithSamePath(string downloadFilePath)
        {
            try
            {
                return _downloads.Values.First(target =>
                    target.FilePath == downloadFilePath && target.IsActive);
            }
            catch (Exception)
            {
                Debug.WriteLine($"(HasDownloadWithSamePath) No match with file {downloadFilePath}");

                return null;
            }
        }

        public Download? Get(string downloadID)
        {
            if (HasDownload(downloadID) &&
                _downloads.TryGetValue(downloadID, out var download))
            {
                return download;
            }

            return null;
        }

        public bool HasDownload(string downloadID)
        {
            return _downloads.ContainsKey(downloadID);
        }

        public bool TryAddDownload(Download download)
        {
            if (!HasDownload(download.ID) &&
                download.TryOpenFile() &&
                _downloads.TryAdd(download.ID, download))
            {
                _downloads[download.ID].Finished += OnDownloadFinished;
                _downloads[download.ID].FileRemoved += OnFileRemoved;
                DownloadsListUpdated?.Invoke(this, EventArgs.Empty);

                return true;
            }

            Debug.WriteLine($"(TryAddDownload) Can't add download {download.ID} into collection");

            return false;
        }

        public void RemoveDownload(string downloadID)
        {
            if (HasDownload(downloadID) &&
                _downloads.TryRemove(downloadID, out Download? removedDownload) &&
                removedDownload != null)
            {
                removedDownload.ShutdownFile();
                removedDownload.Finished -= OnDownloadFinished;
                removedDownload.FileRemoved -= OnFileRemoved;
                DownloadsListUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ShutdownAllDownloads()
        {
            foreach (var download in _downloads.Values)
            {
                download.ShutdownFile();
            }
        }

        public void CancelAllDownloadsFromServer(int serverID)
        {
            var downloadsFromServer = _downloads.Values.Where(download => download.Server.Id == serverID);
            foreach (var download in downloadsFromServer)
            {
                download.Cancel();
            }
        }
    }
}
