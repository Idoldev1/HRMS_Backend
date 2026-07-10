namespace HRMS.API.Common
{
    public enum ResultErrorKind
    {
        Validation,
        NotFound,
        Conflict,
    }

    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }
        public ResultErrorKind ErrorKind { get; }

        private Result(T value)
        {
            IsSuccess = true;
            Value = value;
        }

        private Result(string error, ResultErrorKind kind)
        {
            IsSuccess = false;
            Error = error;
            ErrorKind = kind;
        }

        public static Result<T> Ok(T value) => new(value);

        public static Result<T> Fail(string error) => new(error, ResultErrorKind.Validation);

        public static Result<T> NotFound(string error) => new(error, ResultErrorKind.NotFound);

        public static Result<T> Conflict(string error) => new(error, ResultErrorKind.Conflict);
    }

    public sealed class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public ResultErrorKind ErrorKind { get; }

        private Result(bool success, string? error, ResultErrorKind kind)
        {
            IsSuccess = success;
            Error = error;
            ErrorKind = kind;
        }

        public static Result Ok() => new(true, null, ResultErrorKind.Validation);

        public static Result Fail(string error) => new(false, error, ResultErrorKind.Validation);

        public static Result NotFound(string error) => new(false, error, ResultErrorKind.NotFound);

        public static Result Conflict(string error) => new(false, error, ResultErrorKind.Conflict);
    }
}
