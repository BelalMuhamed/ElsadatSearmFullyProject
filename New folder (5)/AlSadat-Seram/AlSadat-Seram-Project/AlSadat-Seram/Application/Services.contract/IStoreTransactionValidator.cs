using Application.DTOs;
using Domain.Common;

namespace Application.Services.contract
{
    /// <summary>
    /// Validation contract for store-to-store transfer requests.
    /// Returns <see cref="Result{T}"/> with no payload — only success/failure
    /// and a human-readable Arabic message.
    /// </summary>
    /// <remarks>
    /// Why a dedicated contract:
    ///   - Single Responsibility: validation rules can change independently of orchestration.
    ///   - Open/Closed: future rules (e.g. role-based limits) extend this without
    ///     touching the service.
    ///   - Testability: rules are pure-ish (only repository reads) and easy to mock.
    /// </remarks>
    public interface IStoreTransactionValidator
    {
        /// <summary>
        /// Validates a transfer DTO without performing any side effects.
        /// </summary>
        /// <param name="dto">The incoming transfer request.</param>
        /// <returns>
        /// <see cref="Result{T}.Success(T)"/> with <c>true</c> when the request is
        /// well-formed and references existing aggregates; otherwise a Failure
        /// with the appropriate <see cref="System.Net.HttpStatusCode"/>.
        /// </returns>
        Task<Result<bool>> ValidateAsync(StoreTransactionDto dto);
    }
}
