namespace BendigoBank
{
    /// <summary>
    /// Represents a Bendigo Bank BPAY Payee contact.
    /// </summary>
    public sealed class BpayBankContact : BankContact
    {
        public string BillerCode { get; set; }
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Creates a new BPAY Payee contact.
        /// </summary>
        /// <param name="id">An ID for the contact.</param>
        /// <param name="avatar">An avatar for the contact.</param>
        /// <param name="name">A name for the contact.</param>
        /// <param name="lastPaid">Brief information about the last payment date.</param>
        /// <param name="billerCode">The BPAY biller code for the contact.</param>
        /// <param name="referenceNumber">The customer reference number for contact.</param>
        public BpayBankContact(string id, object avatar, string name, string lastPaid, string billerCode, string referenceNumber)
            : base(id, avatar, name, lastPaid)
        {
            BillerCode = billerCode;
            ReferenceNumber = referenceNumber;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format("\nBillerCode={0}, ReferenceNumber={1}", BillerCode, ReferenceNumber);
        }
    }
}
