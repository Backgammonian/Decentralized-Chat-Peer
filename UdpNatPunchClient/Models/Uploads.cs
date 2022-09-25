﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UdpNatPunchClient.Models
{
    public sealed class Uploads
    {
        //<upload ID, upload info>
        private readonly ConcurrentDictionary<string, Upload> _uploads;

        public Uploads()
        {
            _uploads = new ConcurrentDictionary<string, Upload>();
        }

        public event EventHandler<UploadEventArgs>? UploadAdded;
        public event EventHandler<UploadEventArgs>? UploadRemoved;

        public IEnumerable<Upload> UploadsList =>
            _uploads.Values.OrderBy(upload => upload.StartTime);

        public Upload? Get(string uploadID)
        {
            if (Has(uploadID) &&
                _uploads.TryGetValue(uploadID, out var upload))
            {
                return upload;
            }

            return null;
        }

        public bool Has(string uploadID)
        {
            return _uploads.ContainsKey(uploadID);
        }

        public void Add(Upload upload)
        {
            if (_uploads.TryAdd(upload.ID, upload))
            {
                UploadAdded?.Invoke(this, new UploadEventArgs(upload.ID));
            }
        }

        public void Remove(string id)
        {
            if (_uploads.TryRemove(id, out Upload? removedUpload) &&
                removedUpload != null)
            {
                removedUpload.Cancel();
                UploadRemoved?.Invoke(this, new UploadEventArgs(removedUpload.ID));
            }
        }

        public void CancelAllUploadsOfPeer(int peerID)
        {
            var uploads = _uploads.Values.Where(upload => upload.Destination.Id == peerID);

            foreach (var upload in uploads)
            {
                upload.Cancel();
            }
        }
    }
}
