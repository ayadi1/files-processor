using FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;
using MediatR;

namespace FilesProcessor.WebApi.Tests.Mocks;

/// <summary>
/// In-memory stand-in for MediatR's ISender. Records every request it receives
/// and, for GetFileByIdQuery, returns a canned GetFileByIdResult so tests can
/// assert on what a composition handler (e.g. GetTicketByTokenHandler)
/// delegated, without running the real pipeline.
/// </summary>
public class FakeSender : ISender
{
    /// <summary>Captured requests, in the order they were sent.</summary>
    public List<object> SentRequests { get; } = [];

    /// <summary>The result returned when the sent request is a GetFileByIdQuery.</summary>
    public GetFileByIdResult? NextFileResult { get; set; }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        SentRequests.Add(request);

        if (request is GetFileByIdQuery)
        {
            if (NextFileResult is null)
                throw new FileNotFoundException("FakeSender has no GetFileByIdResult prepared.");

            return Task.FromResult((TResponse)(object)NextFileResult);
        }

        throw new NotSupportedException($"FakeSender does not support {request.GetType().Name}.");
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "FakeSender only supports the typed ISender.Send<TResponse> path — the handler under test should use it.");

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
        => throw new NotSupportedException($"FakeSender does not support {typeof(TRequest).Name}.");

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("FakeSender does not support streaming requests.");

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("FakeSender does not support streaming requests.");
}