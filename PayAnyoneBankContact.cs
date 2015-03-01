namespace BendigoBank
{
    /// <summary>
    /// Represents a Bendigo Bank PayAnyone contact.
    /// </summary>
    public sealed class PayAnyoneBankContact : BankContact
    {
        public string Bsb { get; set; }
        public string AccountNumber { get; set; }

        /// <summary>
        /// Creates a new PayAnyone contact.
        /// </summary>
        /// <param name="id">An ID for the contact.</param>
        /// <param name="avatar">An avatar for the contact.</param>
        /// <param name="name">A name for the contact.</param>
        /// <param name="lastPaid">Brief information about the last payment date.</param>
        /// <param name="bsb">A BSB number for the contact.</param>
        /// <param name="accountNumber">An account number for the contact.</param>
        public PayAnyoneBankContact(string id, object avatar, string name, string lastPaid, string bsb, string accountNumber)
            : base(id, avatar, name, lastPaid)
        {
            Bsb = bsb;
            AccountNumber = accountNumber;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format("\nBsb={0}, AccountNumber={1}", Bsb, AccountNumber);
        }
    }
}
