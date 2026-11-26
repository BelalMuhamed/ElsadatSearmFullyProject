using Application.Services.contract.Numbering;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Infrastructure.Services.NumberingServices;

/// <summary>
/// One atomic <c>UPDATE … OUTPUT</c> on the type's <c>DocumentSequences</c> row: SQL Server holds
/// the row's exclusive lock until the caller's transaction ends, so concurrent callers wait in
/// line and each gets the next number; a rollback undoes the increment (D31).
/// </summary>
public sealed class DocumentNumberService(AppDbContext context) : IDocumentNumberService
{
    /// <inheritdoc />
    public async Task<string> NextAsync(DocumentType type, CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException(
                "IDocumentNumberService.NextAsync must run inside the transaction that saves the document (D31).");

        var typeValue = (byte)type;
        var rows = await context.Database
            .SqlQuery<NextNumber>($"""
                UPDATE [dbo].[DocumentSequences]
                SET [LastNumber] = [LastNumber] + 1
                OUTPUT inserted.[Prefix], inserted.[LastNumber], inserted.[MinDigits]
                WHERE [DocumentType] = {typeValue}
                """)
            .ToListAsync(cancellationToken);

        var next = rows.SingleOrDefault()
            ?? throw new InvalidOperationException($"No DocumentSequences row for {type}: seed it with the enum member.");

        return $"{next.Prefix}-{next.LastNumber.ToString("D" + next.MinDigits, CultureInfo.InvariantCulture)}";
    }

    private sealed record NextNumber(string Prefix, int LastNumber, byte MinDigits);
}
