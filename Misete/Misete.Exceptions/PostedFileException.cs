namespace Misete.Exceptions
{
    public class PostedFileException : System.Exception
    {
        public PostedFileException()
        {
        }

        public PostedFileException(string message)
            : base(message)
        {
        }

        public PostedFileException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}
