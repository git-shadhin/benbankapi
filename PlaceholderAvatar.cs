namespace BendigoBank
{
    /// <summary>
    /// Represents a placeholder avatar for a contact.
    /// </summary>
    public sealed class PlaceholderAvatar
    {
        public string Initials { get; set; }
        public string Colour { get; set; }

        public override string ToString()
        {
            return string.Format("Initials={0}, Colour={1}", Initials, Colour);
        }
    }
}
