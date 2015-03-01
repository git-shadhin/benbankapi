namespace BendigoBank
{
    /// <summary>
    /// A class which represents a Bendigo Bank contact.
    /// </summary>
    public class BankContact
    {
        public string Id { get; set; }
        public object Avatar { get; set; } // This is of type object so that it may be implemented as either a string or a more complex type.
        public string Name { get; set; }
        public string LastPaid { get; set; }

        /// <summary>
        /// Creates a new BankContact object.
        /// </summary>
        /// <param name="id">An ID string for the contact.</param>
        /// <param name="avatar">An avatar for the contact.</param>
        /// <param name="name">A name for the contact.</param>
        /// <param name="lastPaid">Brief information about the last payment date.</param>
        public BankContact(string id, object avatar, string name, string lastPaid)
        {
            Id = id;
            Avatar = avatar;
            Name = name;
            LastPaid = lastPaid;
        }

        public override string ToString()
        {
            return string.Format("Id={0}, Name={1}, LastPaid={2}, Avatar={3}", Id, Name, LastPaid, Avatar);
        }
    }
}
