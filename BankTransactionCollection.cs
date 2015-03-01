using System;
using System.Collections.Generic;
using System.Linq;

namespace BendigoBank
{
    /// <summary>
    /// An IEnumerable collection of BankTransaction objects.
    /// </summary>
    public sealed class BankTransactionCollection : IEnumerable<BankTransaction>
    {
        private List<BankTransaction> _transactions;

        /// <summary>
        /// Creates a new BankTransactionCollection.
        /// </summary>
        public BankTransactionCollection()
        {
            _transactions = new List<BankTransaction>();
        }

        /// <summary>
        /// Adds a new BankTransaction to the collection.
        /// </summary>
        /// <param name="transaction">A BankTransaction to add to the collection.</param>
        public void Add(BankTransaction transaction)
        {
            _transactions.Add(transaction);
        }

        /// <summary>
        /// Retrieves the first BankTransaction with the name specified.
        /// </summary>
        /// <param name="name">A name for the transaction to be retrieved.</param>
        /// <returns>The first BankTransaction with the name specified.</returns>
        public BankTransaction this[int index]
        {
            get { return _transactions[index]; }
        }

        /// <summary>
        /// Retrieves the BankTransaction at the zero-based index specified.
        /// </summary>
        /// <param name="index">The zero-based index of the transaction to retrieve.</param>
        /// <returns>The BankTransaction at the zero-based index specified.</returns>
        public IEnumerator<BankTransaction> GetEnumerator()
        {
            return _transactions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _transactions.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _transactions);
        }
    }
}
