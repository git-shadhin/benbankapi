using System;
using System.Collections.Generic;
using System.Linq;

namespace BendigoBank
{
    /// <summary>
    /// An IEnumerable collection of BankAccount objects.
    /// </summary>
    public sealed class BankAccountCollection : IEnumerable<BankAccount>
    {
        private List<BankAccount> _accounts;

        /// <summary>
        /// Creates a new BankAccountCollection.
        /// </summary>
        public BankAccountCollection()
        {
            _accounts = new List<BankAccount>();
        }

        /// <summary>
        /// Adds a new BankAccount to the collection.
        /// </summary>
        /// <param name="account">A BankAccount to add to the collection.</param>
        public void Add(BankAccount account)
        {
            _accounts.Add(account);
        }

        /// <summary>
        /// Retrieves the first BankAccount with the name specified.
        /// </summary>
        /// <param name="name">A name for the account to be retrieved.</param>
        /// <returns>The first BankAccount with the name specified.</returns>
        public BankAccount this[string name]
        {
            get { return _accounts.FirstOrDefault(a => a.Name == name); }
        }

        /// <summary>
        /// Retrieves the BankAccount at the zero-based index specified.
        /// </summary>
        /// <param name="index">The zero-based index of the account to retrieve.</param>
        /// <returns>The BankAccount at the zero-based index specified.</returns>
        public BankAccount this[int index]
        {
            get { return _accounts[index]; }
        }

        public IEnumerator<BankAccount> GetEnumerator()
        {
            return _accounts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _accounts.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _accounts);
        }
    }
}
