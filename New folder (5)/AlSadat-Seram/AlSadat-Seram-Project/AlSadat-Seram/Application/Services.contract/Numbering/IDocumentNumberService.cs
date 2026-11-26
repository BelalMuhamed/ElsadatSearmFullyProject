using Domain.Enums;

namespace Application.Services.contract.Numbering;

/// <summary>Hands out document numbers like <c>INV-1038</c> (F4, D31–D34).</summary>
public interface IDocumentNumberService
{
    /// <summary>
    /// Takes the next number of <paramref name="type"/>. Must be called inside the transaction that
    /// saves the document: the counter row stays locked until it commits, so two requests never get
    /// the same number, and a rollback gives the number back. Throws
    /// <see cref="InvalidOperationException"/> when no transaction is open.
    /// </summary>
    /// <param name="type">The kind of document.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>The formatted number, e.g. <c>INV-0001</c>.</returns>
    Task<string> NextAsync(DocumentType type, CancellationToken cancellationToken = default);
}
