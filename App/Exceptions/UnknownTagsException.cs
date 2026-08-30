namespace App.Exceptions
{
    public class UnknownTagsException : AppException
    {
        public IReadOnlyCollection<string> InvalidTags => invalidTags;

        string[] invalidTags;

        public UnknownTagsException(string message, IEnumerable<string> tags) : base(message)
        {
            invalidTags = tags.ToArray();
        }
    }
}
