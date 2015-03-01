using System;
using System.Collections.Generic;
using System.Linq;

namespace BendigoBank
{
    /// <summary>
    /// An IEnumerable collection of BankContact objects.
    /// </summary>
    public sealed class BankContactCollection : IEnumerable<BankContact>
    {
        private List<BankContact> _contacts;

        /// <summary>
        /// Creates a new BankContactCollection.
        /// </summary>
        public BankContactCollection()
        {
            _contacts = new List<BankContact>();
        }

        /// <summary>
        /// Adds a new BankContact to the collection.
        /// </summary>
        /// <param name="contact">A BankContact to add to the collection.</param>
        public void Add(BankContact contact)
        {
            _contacts.Add(contact);
        }

        /// <summary>
        /// Retrieves the first BankContact with the name specified.
        /// </summary>
        /// <param name="name">A name for the contact to be retrieved.</param>
        /// <returns>The first BankContact with the name specified.</returns>
        public BankContact this[string name]
        {
            get { return _contacts.FirstOrDefault(c => c.Name == name); }
        }

        /// <summary>
        /// Retrieves the BankContact at the zero-based index specified.
        /// </summary>
        /// <param name="index">The zero-based index of the contact to retrieve.</param>
        /// <returns>The BankContact at the zero-based index specified.</returns>
        public BankContact this[int index]
        {
            get { return _contacts[index]; }
        }

        public IEnumerator<BankContact> GetEnumerator()
        {
            return _contacts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _contacts.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _contacts);
        }
    }
}
