namespace Ecommerce.Application.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
        public List<string>? Errors { get; init; }

        public static Result<T> Success(T data, string? message = null)
        => new() { IsSuccess = true, Data = data, Message = message };

        public static Result<T> Failure(string error)
            => new() { IsSuccess = false, Errors = new List<string> { error } };

        public static Result<T> Failure(List<string> errors)
            => new() { IsSuccess = false, Errors = errors };
    }
}
