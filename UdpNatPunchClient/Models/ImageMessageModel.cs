using System.Diagnostics;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class ImageMessageModel : BaseMessageModel
    {
        private ImageItem? _image;
        private bool _isImageLoaded;
        private bool _isFailed;

        //outgoing
        public ImageMessageModel() : base()
        {
            IsImageLoaded = false;
            IsFailed = false;
        }

        //incoming
        public ImageMessageModel(ImageIntroduceMessage messageFromOutside) : base(messageFromOutside.MessageID)
        {
            IsImageLoaded = false;
            IsFailed = false;
        }

        public bool IsImageNotLoaded => !IsImageLoaded;

        public bool IsImageLoaded
        {
            get => _isImageLoaded;
            private set
            {
                SetProperty(ref _isImageLoaded, value);
                OnPropertyChanged(nameof(IsImageNotLoaded));
            }
        }

        public bool IsFailed
        {
            get => _isFailed;
            private set => SetProperty(ref _isFailed, value);
        }

        public ImageItem? Image
        {
            get => _image;
            private set => SetProperty(ref _image, value);
        }

        public void UpdateImage(ImageItem image)
        {
            if (IsFailed)
            {
                return;
            }

            if (image != null &&
                image.IsLoaded)
            {
                IsImageLoaded = true;
                Image = image;
            }
            else
            {
                Debug.WriteLine("(UpdateImage) Can't update message image");
            }
        }

        public void SetAsFailed()
        {
            IsFailed = true;
        }
    }
}
