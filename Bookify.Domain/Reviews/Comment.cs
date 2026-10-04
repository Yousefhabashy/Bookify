using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Reviews
{
    public record Comment
    {
        public string Value { get; }

        private Comment(string value) => Value = value;

        public static Result<Comment> Create(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return Result.Failure<Comment>(new Error("Comment.Empty", "Review comment cannot be empty."));
            }

            if (comment.Length > 400)
            {
                return Result.Failure<Comment>(new Error("Comment.TooLong", "Review comment cannot exceed 400 characters."));
            }

            return new Comment(comment);
        }
    }
}
