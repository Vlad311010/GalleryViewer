namespace App.Exceptions
{
    internal class NotSupportedMimeTypeException : AppException
    {
        public string MimeType { get; }

        public NotSupportedMimeTypeException(string mimeType) : base($"Mime type '{mimeType}' is not supported")
        {
            MimeType = mimeType;
        }
    }
}
