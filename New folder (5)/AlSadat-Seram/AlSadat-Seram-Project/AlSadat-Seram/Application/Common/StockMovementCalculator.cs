namespace Application.Common
{
    /// <summary>
    /// Pure, side-effect-free helpers for stock-transfer cost mathematics.
    /// <para>
    /// Centralising this formula in one place is critical: average cost is the
    /// foundation of inventory valuation and any drift between the transfer flow
    /// and the purchase-invoice flow would corrupt financial reports.
    /// </para>
    /// <para>
    /// All methods here are <b>pure</b>: no DB, no DI, no mutation of inputs.
    /// They are trivially unit-testable.
    /// </para>
    /// </summary>
    public static class StockMovementCalculator
    {
        /// <summary>
        /// Computes the new weighted-average cost at the destination warehouse
        /// after receiving <paramref name="transferQuantity"/> units that were
        /// valued at <paramref name="sourceAvgCost"/> per unit.
        /// </summary>
        /// <param name="destinationCurrentQuantity">
        /// Quantity already on hand at the destination BEFORE the transfer.
        /// May be zero (first time this product enters the destination).
        /// </param>
        /// <param name="destinationCurrentAvgCost">
        /// Existing weighted-average cost at the destination. Ignored when
        /// <paramref name="destinationCurrentQuantity"/> is zero.
        /// </param>
        /// <param name="transferQuantity">
        /// Quantity being moved. Must be positive — caller validates.
        /// </param>
        /// <param name="sourceAvgCost">
        /// Weighted-average cost at the source warehouse at the moment of transfer.
        /// </param>
        /// <returns>
        /// The new weighted-average cost the destination should record.
        /// If both quantities are zero (defensive case) returns <paramref name="sourceAvgCost"/>.
        /// </returns>
        public static decimal ComputeNewDestinationAvgCost(
            decimal destinationCurrentQuantity,
            decimal destinationCurrentAvgCost,
            decimal transferQuantity,
            decimal sourceAvgCost)
        {
            // First arrival of this product at the destination.
            // The destination's avg-cost simply equals the source's avg-cost.
            if (destinationCurrentQuantity <= 0m)
                return sourceAvgCost;

            var totalQuantity = destinationCurrentQuantity + transferQuantity;

            // Defensive: should never happen because callers validate quantity > 0,
            // but guards against a divide-by-zero if a bug ever slips through.
            if (totalQuantity <= 0m)
                return sourceAvgCost;

            var totalCost =
                (destinationCurrentQuantity * destinationCurrentAvgCost) +
                (transferQuantity * sourceAvgCost);

            return totalCost / totalQuantity;
        }
    }
}