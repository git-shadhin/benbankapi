using System;

namespace BendigoBank
{
    public class BankTransfer
    {
        public float Amount { get; set; }
        public string Description { get; set; }
        public string PaymentDate { get; set; }
        public string SecurityToken { get; set; }

        /// <summary>
        /// Creates a new BankTransfer object.
        /// </summary>
        /// <param name="amount">An amount of money to be transferred.</param>
        /// <param name="description">A description for the transfer.</param>
        /// <param name="paymentDate">An ISO-8601 date upon which the transfer should take place.</param>
        /// <param name="securityToken">A 6-digit security token.</param>
        public BankTransfer(float amount, string description, string paymentDate, string securityToken)
        {
            if (amount > 0)
            {
                if (description.Length < 19)
                {
                    DateTime payDate = DateTime.Parse(paymentDate);
                    int daysFromNow = payDate.Subtract(DateTime.Now).Days;
                    if (daysFromNow > 0 && daysFromNow < 366)
                    {
                        if (securityToken.Length == 6)
                        {
                            Amount = amount;
                            Description = description;
                            PaymentDate = paymentDate;
                            SecurityToken = securityToken;
                        }
                        else
                            throw new Exception("Security token must be 6 digits long.");
                    }
                    else
                        throw new Exception("Future payment date must be no more than a year in the future.");
                }
                else
                    throw new Exception("Description must not be longer than 18 characters.");
            }
            else
                throw new Exception("Transfer amount must be greater than zero.");
        }

        /// <summary>
        /// Creates a new BankTransfer object.
        /// </summary>
        /// <param name="amount">An amount of money to be transferred.</param>
        /// <param name="description">A description for the transfer.</param>
        /// <param name="paymentDate">An ISO-8601 date upon which the transfer should take place.</param>
        public BankTransfer(float amount, string description, string paymentDate)
        {
            if (amount > 0)
            {
                if (description.Length < 19)
                {
                    DateTime payDate = DateTime.Parse(paymentDate);
                    int daysFromNow = payDate.Subtract(DateTime.Now).Days;
                    if (daysFromNow > 0 && daysFromNow < 366)
                    {
                        Amount = amount;
                        Description = description;
                        PaymentDate = paymentDate;
                        SecurityToken = string.Empty;
                    }
                    else
                        throw new Exception("Future payment date must be no more than a year in the future.");
                }
                else
                    throw new Exception("Description must not be longer than 18 characters.");
            }
            else
                throw new Exception("Transfer amount must be greater than zero.");
        }

        /// <summary>
        /// Creates a new BankTransfer object.
        /// </summary>
        /// <param name="amount">An amount of money to be transferred.</param>
        /// <param name="description">A description for the transfer.</param>
        public BankTransfer(float amount, string description)
        {
            if (amount > 0)
            {
                if (description.Length < 19)
                {
                    Amount = amount;
                    Description = description;
                    PaymentDate = string.Empty;
                    SecurityToken = string.Empty;
                }
                else
                    throw new Exception("Description must not be longer than 18 characters.");
            }
            else
                throw new Exception("Transfer amount must be greater than zero.");
        }

        /// <summary>
        /// Creates a new BankTransfer object.
        /// </summary>
        /// <param name="amount">An amount of money to be transferred.</param>
        public BankTransfer(float amount)
        {
            if (amount > 0)
            {
                Amount = amount;
                Description = string.Empty;
                PaymentDate = string.Empty;
                SecurityToken = string.Empty;
            }
            else
                throw new Exception("Transfer amount must be greater than zero.");
        }

        public override string ToString()
        {
            return string.Format("Amount={0}, Desc={1}, PaymentDate={2}", Amount, Description, PaymentDate);
        }
    }
}
