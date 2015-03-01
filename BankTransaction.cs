namespace BendigoBank
{
    public class BankTransaction
    {
        // The members of this class use a specific naming convention
        // so that they may be assigned by using a deserialisation function
        // of the JSON.Net library.
        public string id { get; set; }
        public int amount_in_cents { get; set; }
        public string type { get; set; }
        public string created_at { get; set; }
        public string description { get; set; }
        public int running_balance_in_cents { get; set; }

        public BankTransaction()
        {
            id = string.Empty;
            amount_in_cents = 0;
            type = string.Empty;
            created_at = string.Empty;
            description = string.Empty;
            running_balance_in_cents = 0;
        }

        public override string ToString()
        {
            return string.Format("id={0}, amount={1}, type={2}, created={3}, desc={4}, running={5}",
                id, amount_in_cents, type, created_at, description, running_balance_in_cents);
        }
    }
}