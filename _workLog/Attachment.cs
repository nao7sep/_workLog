using System.Text;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace _workLog
{
    public class Attachment
    {
        public Guid Id { get; init; }

        public Guid MessageId { get; init; }

        public DateTime CreatedAtUtc { get; init; }

        public required string RelativePath { get; init; }

        private string? _relativeUrl;

        [JsonIgnore]
        public string RelativeUrl => _relativeUrl ??= RelativePath.Replace ('\\', '/');

        private string? _path;

        [JsonIgnore]
        public string Path => _path ??= Environment.MapPath (RelativePath);

        private string? _name;

        [JsonIgnore]
        public string Name => _name ??= System.IO.Path.GetFileName (RelativePath);

        private long? _length;

        // Can be deserialized from JSON to reduce disk I/O.
        public long Length
        {
            get => _length ??= new FileInfo (Path).Length;
            set => _length = value;
        }

        // Image-related info should always be deserialized from JSON for efficiency.
        // Path-related things that can be regenerated at no cost will not be deserialized.

        private bool? _isImage;

        // A simple yes/no question, which will always have an answer after _tryHandleImage is called.
        public bool IsImage
        {
            get
            {
                if (_isImage is null)
                    _tryHandleImage ();

                return _isImage!.Value;
            }

            set => _isImage = value;
        }

        // All image-related nullable properties return null if IsImage is false.

        // _tryHandleImage does its job only once.
        // From then on, unneeded calls to it will be skipped internally.

        private IImageFormat? _imageFormat;

        public IImageFormat? ImageFormat
        {
            get
            {
                if (_imageFormat is null)
                    _tryHandleImage ();

                return _imageFormat;
            }

            set => _imageFormat = value;
        }

        private Size? _imageSize;

        public Size? ImageSize
        {
            get
            {
                if (_imageSize is null)
                    _tryHandleImage ();

                return _imageSize;
            }

            set => _imageSize = value;
        }

        private string? _resizedImageRelativePath;

        public string? ResizedImageRelativePath
        {
            get
            {
                if (_resizedImageRelativePath is null)
                    _tryHandleImage ();

                return _resizedImageRelativePath;
            }

            set => _resizedImageRelativePath = value;
        }

        private string? _resizedImageRelativeUrl;

        [JsonIgnore]
        public string? ResizedImageRelativeUrl => _resizedImageRelativeUrl ??= ResizedImageRelativePath is null ? null : ResizedImageRelativePath.Replace ('\\', '/');

        private string? _resizedImagePath;

        [JsonIgnore]
        public string? ResizedImagePath => _resizedImagePath ??= ResizedImageRelativePath is null ? null : Environment.MapPath (ResizedImageRelativePath);

        private long? _resizedImageLength;

        public long? ResizedImageLength
        {
            get
            {
                if (_resizedImageLength is null)
                    _tryHandleImage (); // It's simpler to let _tryHandleImage handle all image-related properties.

                return _resizedImageLength;
            }

            set => _resizedImageLength = value;
        }

        private Size? _resizedImageSize;

        public Size? ResizedImageSize
        {
            get
            {
                if (_resizedImageSize is null)
                    _tryHandleImage ();

                return _resizedImageSize;
            }

            set => _resizedImageSize = value;
        }

        private void _tryHandleImage ()
        {
            try
            {
                if (_isImage is not null)
                    return;

                using Image xImage = Image.Load (Path);

                // https://docs.sixlabors.com/articles/imagesharp/imageformats.html
                // https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Metadata.ImageMetadata.html#SixLabors_ImageSharp_Metadata_ImageMetadata_DecodedImageFormat

                if (xImage.Metadata.DecodedImageFormat is null)
                    throw new Exception ("Image format is unknown.");

                Size xImageSize = xImage.Size;

                try
                {
                    // https://docs.sixlabors.com/articles/imagesharp/gettingstarted.html
                    // https://docs.sixlabors.com/articles/imagesharp/processing.html
                    // https://docs.sixlabors.com/articles/imagesharp/resize.html
                    // https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.ResizeOptions.html
                    // https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.ResizeMode.html

                    if (xImage.Width > 1024 || xImage.Height > 1024)
                    {
                        xImage.Mutate (x => x.Resize (new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new (1024, 1024)
                        }));
                    }

                    string xResizedDirectoryPath = System.IO.Path.Join (System.IO.Path.GetDirectoryName (Path), "Resized");
                    Directory.CreateDirectory (xResizedDirectoryPath);

                    // Important: If we eventually choose to create 2 or more resized images here, ALL of them will have to contain an unique word in their name.
                    // If we make Hoge-large.jpg and Hoge.jpg for Hoge.jpg for example, uploading Hoge-large.jpg will overwrite the existing Hoge-large.jpg.

                    string xResizedImageRelativePath = System.IO.Path.Join (System.IO.Path.GetDirectoryName (RelativePath), "Resized", Name),
                        xResizedImagePath = Environment.MapPath (xResizedImageRelativePath);

                    // https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Formats.ImageFormatManager.html
                    // https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Formats.IImageEncoder.html

                    var xEncoder = xImage.Configuration.ImageFormatsManager.GetEncoder (xImage.Metadata.DecodedImageFormat);
                    xImage.Save (xResizedImagePath, xEncoder);

                    _isImage = true;
                    _imageFormat = xImage.Metadata.DecodedImageFormat;
                    _imageSize = xImageSize;
                    _resizedImageRelativePath = xResizedImageRelativePath;
                    // _resizedImagePath = xResizedImagePath;
                    _resizedImageLength = new FileInfo (xResizedImagePath).Length;
                    _resizedImageSize = xImage.Size;
                }

                catch // Failure upon resizing the image.
                {
                    string xResizedDirectoryPath = System.IO.Path.Join (System.IO.Path.GetDirectoryName (Path), "Resized");

                    if (Directory.Exists (xResizedDirectoryPath) && Directory.GetFileSystemEntries (xResizedDirectoryPath).Length == 0)
                        Directory.Delete (xResizedDirectoryPath);

                    _isImage = false;
                }
            }

            catch // Failure upon loading the image (or deleting the Resized directory).
            {
                _isImage = false;
            }
        }

    #if DEBUG
        public void DisplayAllInfo ()
        {
            StringBuilder xBuilder = new ();

            xBuilder.AppendLine ($"Id: {Id}");
            xBuilder.AppendLine ($"MessageId: {MessageId}");
            xBuilder.AppendLine ($"CreatedAtUtc: {CreatedAtUtc}");
            xBuilder.AppendLine ($"RelativePath: {RelativePath}");
            xBuilder.AppendLine ($"RelativeUrl: {RelativeUrl}");
            xBuilder.AppendLine ($"Path: {Path}");
            xBuilder.AppendLine ($"Name: {Name}");
            xBuilder.AppendLine ($"Length: {Length}");
            xBuilder.AppendLine ($"IsImage: {IsImage}");

            if (IsImage)
            {
                xBuilder.AppendLine ($"ImageFormat: {ImageFormat}");
                xBuilder.AppendLine ($"ImageSize: {ImageSize}");
                xBuilder.AppendLine ($"ResizedImageRelativePath: {ResizedImageRelativePath}");
                xBuilder.AppendLine ($"ResizedImageRelativeUrl: {ResizedImageRelativeUrl}");
                xBuilder.AppendLine ($"ResizedImagePath: {ResizedImagePath}");
                xBuilder.AppendLine ($"ResizedImageLength: {ResizedImageLength}");
                xBuilder.AppendLine ($"ResizedImageSize: {ResizedImageSize}");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write (xBuilder);
            Console.ResetColor ();
        }
    #endif
    }
}
