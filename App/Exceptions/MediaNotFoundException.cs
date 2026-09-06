namespace App.Exceptions
{
    public class MediaNotFoundException : AppException
    {
        public string? MeadiaPath { get; }

        public MediaNotFoundException(string message, string? mediaPath) : base(message)
        {
            MeadiaPath = mediaPath;
        }
    }
}
