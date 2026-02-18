namespace DigitalTwinPlatform.Domain.Common;

public abstract record Result
{
    public sealed record Success : Result;
    public sealed record Failure(string Error) : Result;

    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<string, TResult> onFailure) =>
        this switch
        {
            Success => onSuccess(),
            Failure f => onFailure(f.Error),
            _ => throw new InvalidOperationException()
        };
}

public abstract record Result<T>
{
    public sealed record Success(T Value) : Result<T>;
    public sealed record Failure(string Error) : Result<T>;

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Value),
            Failure f => onFailure(f.Error),
            _ => throw new InvalidOperationException()
        };

    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure) =>
        this switch
        {
            Success s => await onSuccess(s.Value),
            Failure f => await onFailure(f.Error),
            _ => throw new InvalidOperationException()
        };
}
