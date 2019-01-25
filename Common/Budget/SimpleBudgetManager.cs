using System.Threading.Tasks;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// This is terrible
    /// </summary>
    public class SimpleBudgetManager : IBudgetManager
    {
        decimal _budget = 0m;

        /// <summary>
        /// Useless
        /// </summary>
        public SimpleBudgetManager()
        {

        }

        /// <inheritdoc/>
        public async Task<decimal> GetAdditional()
        {
            return await Task.FromResult(_budget);
        }

        /// <inheritdoc/>
        public async Task<decimal> GetAdditional(decimal amount)
        {
            return await Task.FromResult(_budget);
        }

        /// <inheritdoc/>
        public async Task<decimal> GetRemaining()
        {
            return await Task.FromResult(_budget);
        }

        /// <inheritdoc/>
        public bool IsExhausted()
        {
            if (_budget <= 0m)
            {
                GetAdditional().ContinueWith(t =>
                {
                    _budget += t.Result + .01m;
                });
            }
            
            return _budget <= 0m;
        }

        /// <inheritdoc/>
        public async Task<bool> TrySpend(decimal amount)
        {
            return await Task.FromResult(false);
        }
    }
}