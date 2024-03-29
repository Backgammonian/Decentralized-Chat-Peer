﻿using System;
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
                Debug.WriteLine($"(GetDownloadWithSamePath) No match with file {downloadFilePath}");

                return null;
            }
        }

        public Download? Get(string downloadID)
        {
            if (_downloads.TryGetValue(downloadID, out var download))
            {
                return download;
            }

            return null;
        }

        public bool TryAddDownload(Download download)
        {
            if (download.TryOpenFile() &&
                _downloads.TryAdd(download.DownloadID, download))
            {
                _downloads[download.DownloadID].Finished += OnDownloadFinished;
                _downloads[download.DownloadID].FileRemoved += OnFileRemoved;
                DownloadsListUpdated?.Invoke(this, EventArgs.Empty);

                return true;
            }
            else
            {
                Debug.WriteLine($"(TryAddDownload) Can't add download {download.DownloadID} into collection");

                download.ShutdownFile();

                return false;
            }
        }

        public void RemoveDownload(string downloadID)
        {
            if (_downloads.TryRemove(downloadID, out Download? removedDownload) &&
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
            var downloadsFromServer = _downloads.Values.Where(download => download.Server.PeerID == serverID);
            foreach (var download in downloadsFromServer)
            {
                download.Cancel();
            }
        }
    }
}
