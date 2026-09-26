namespace App.Exceptions
{
    public class MediaNotFoundException : AppException
    {
        public string? MediaPath { get; }

        public MediaNotFoundException(string message, string? mediaPath) : base(message)
        {
            MediaPath = mediaPath;
        }
    }
}
