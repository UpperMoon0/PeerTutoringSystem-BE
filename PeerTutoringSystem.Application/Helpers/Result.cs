using System;
using System.Collections.Generic;
using System.Linq;

namespace PeerTutoringSystem.Application.Helpers
{
    public class Result<T>
    {
        public T Value { get; }
        public bool IsSuccess { get; }
        public string Error { get; }
        public IEnumerable<string> Errors { get; }

        private Result(T value, bool isSuccess, string error, IEnumerable<string> errors = null)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, null);
        public static Result<T> Failure(string error) => new Result<T>(default(T), false, error);
        public static Result<T> Failure(IEnumerable<string> errors) => new Result<T>(default(T), false, string.Join("; ", errors), errors);
    }
}