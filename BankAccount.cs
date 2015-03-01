using BendigoBank.DataVisualisation;

namespace BendigoBank
{
    /// <summary>
    /// Represents a Bednigo Bank bank account.
    /// </summary>
    public class BankAccount
    {
        public string Holder { get; set; }
        public string Name { get; set; }
        public string Bsb { get; set; }
        public string Number { get; set; }
        public float Current { get; set; }
        public float Available { get; set; }
        public string Id { get; set; }
        public LineChart ChartData { get; set; }

        /// <summary>
        /// Creates a new BankAccount object.
        /// </summary>
        public BankAccount()
        {
            Holder = string.Empty;
            Id = string.Empty;
            Name = string.Empty;
            Bsb = string.Empty;
            Number = string.Empty;
            Available = 0;
            ChartData = null;
        }

        /// <summary>
        /// Creates a new BankAccount object.
        /// </summary>
        /// <param name="holder">The name of the account holder.</param>
        /// <param name="id">An ID for the account.</param>
        /// <param name="name">A name for the account.</param>
        /// <param name="bsb">A BSB number for the account.</param>
        /// <param name="number">An account number for the account.</param>
        /// <param name="current">A current balance for the account.</param>
        /// <param name="available">An available balance for the account.</param>
        public BankAccount(string holder, string id, string name, string bsb, string number, float current, float available)
        {
            Holder = holder;
            Id = id;
            Name = name;
            Bsb = bsb;
            Number = number;
            Current = current;
            Available = available;
            ChartData = null;
        }

        /// <summary>
        /// Creates a new BankAccount object.
        /// </summary>
        /// <param name="holder">The name of the account holder.</param>
        /// <param name="id">An ID for the account.</param>
        /// <param name="name">A name for the account.</param>
        /// <param name="bsb">A BSB number for the account.</param>
        /// <param name="number">An account number for the account.</param>
        /// <param name="current">A current balance for the account.</param>
        /// <param name="available">An available balance for the account.</param>
        /// <param name="recentTransactions">A collection of transactions from which line chart data can be generated.</param>
        public BankAccount(string holder, string id, string name, string bsb, string number, float current, float available, BankTransactionCollection recentTransactions)
        {
            Holder = holder;
            Id = id;
            Name = name;
            Bsb = bsb;
            Number = number;
            Current = current;
            Available = available;
            ChartData = new LineChart(recentTransactions);
        }

        public override string ToString()
        {
            return string.Format("Holder={0}, Name={1}, Bsb={2}, Number={3}, Current={4}, Available={5}, Id={6}, ChartData={7}",
                Holder, Name, Bsb, Number, Current, Available, Id, ChartData);
        }
    }
}
