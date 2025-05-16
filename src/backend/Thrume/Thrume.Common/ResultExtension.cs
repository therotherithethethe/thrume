namespace Thrume.Common;

public static class ResultExtension
{
    public static Result<T> ToResult<T>(this T? entity, ProblemDetails detailsOnError) =>
        entity is null ? Result<T>.Failure(detailsOnError) : Result<T>.Success(entity);
    
    public static void Match<T>(this Result<T> result, Action<T> onSuccess, Action<List<ProblemDetails>> onError)
    {
        if(result.IsSuccess) onSuccess(result.Value!);
        else onError(result.Errors);
    }

    public static K Match<T, K>(this Result<T> result, Func<T, K> onSuccess, Func<List<ProblemDetails>, K> onError) =>
        result.IsSuccess ? onSuccess(result.Value!) : onError(result.Errors);
}