using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Thrume.Common;

public readonly struct Result<T>
{
    public T? Value { get; }
    public List<ProblemDetails> Errors { get; } = [];
    public bool IsSuccess => Errors is [];
    public bool IsFault => !IsSuccess;

    private Result(T value) => Value = value;
    private Result(params ReadOnlySpan<ProblemDetails> errors) => Errors.AddRange(errors);
    
    public static Result<T> Success(T entity) => new(entity);
    [OverloadResolutionPriority(1)]
    public static Result<T> Failure(params ReadOnlySpan<ProblemDetails> errors) => new(errors);
    public static Result<T> Failure(params List<ProblemDetails> errors) => new(CollectionsMarshal.AsSpan(errors));
    public static Result<T> Failure(ProblemDetails error) => new(error);

    public static implicit operator Result<T>(T entity) => Success(entity);
    public static implicit operator Result<T>(ProblemDetails error) => Failure(error);
}