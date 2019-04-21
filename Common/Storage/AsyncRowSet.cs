using System.Collections;
using System.Collections.Generic;
using Cassandra;
using Lucent.Common.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Asynchronous filling rowset enumerator
    /// </summary>
    public class AsyncRowSet : IEnumerator<Row>, IEnumerable<Row>
    {
        IRingBuffer<Row> _rows;
        IEnumerator<Row> _enum;

        /// <summary>
        /// Asynchronous rowset enumerator
        /// </summary>
        /// <param name="original"></param>
        public AsyncRowSet(RowSet original)
        {
            // Create and open the buffer, assign the enum for wrapping
            _rows = new RingBuffer<Row>();
            _rows.Open();
            _enum = _rows.GetEnumerator();

            // Appreciate the warning, but this is done on purpose...
#pragma warning disable
            fillBuffer(original);
#pragma warning restore
        }

        /// <summary>
        /// Asynchronously fill the buffer
        /// </summary>
        /// <param name="rowSet">The rowset to fill the buffer with</param>
        /// <returns></returns>
        async Task fillBuffer(RowSet rowSet)
        {
            var numRows = 0;

            using (var rowEnum = rowSet.GetEnumerator())
            {
                while (!rowSet.IsFullyFetched)
                    if ((numRows = rowSet.GetAvailableWithoutFetching()) > 0)
                        for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                            while (!_rows.TryAdd(rowEnum.Current))
                                await Task.Delay(100);
                    else
                        await rowSet.FetchMoreResultsAsync();

                while (rowEnum.MoveNext())
                    while (!_rows.TryAdd(rowEnum.Current))
                        await Task.Delay(100);
            }

            _rows.Close();
        }

        /// <inheritdoc/>
        public Row Current => _enum.Current;

        /// <inheritdoc/>
        object IEnumerator.Current => _enum.Current;

        /// <inheritdoc/>
        public void Dispose() => _enum.Dispose();

        /// <inheritdoc/>
        public bool MoveNext() => _enum.MoveNext();

        /// <inheritdoc/>
        public void Reset() => _enum.Reset(); // Will blow up...

        /// <inheritdoc/>
        public IEnumerator<Row> GetEnumerator() => this;

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}