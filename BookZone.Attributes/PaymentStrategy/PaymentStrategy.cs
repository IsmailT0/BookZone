using System;

namespace BookZone.Attributes.PaymentStrategy
{
    public abstract class PaymentStrategy
    {

        // Process payment logic for each payment method
        public abstract bool ProcessPayment(decimal amount, decimal receivedAmount);


        // Execute payment process for each payment method template method
        public bool ExecutePayment(decimal amount, decimal receivedAmount)
        {
            Console.WriteLine($"Starting payment process using {this.GetType().Name}");

            if (!ValidateAmount(amount, receivedAmount))
            {
                Console.WriteLine("Amount validation failed.");
                return false;
            }

            if (!ProcessPayment(amount, receivedAmount))
            {
                Console.WriteLine("Payment-specific logic failed.");
                return false;
            }

            LogPayment(amount, receivedAmount);

            Console.WriteLine("Payment process completed successfully.");
            return true;
        }

        // Common payment validation logic
        private bool ValidateAmount(decimal amount, decimal receivedAmount)
        {
            Console.WriteLine($"Validating amount. Expected: {amount}, Received: {receivedAmount}");
            return receivedAmount >= amount;
        }

        // Common payment logging logic
        private void LogPayment(decimal amount, decimal receivedAmount)
        {
            Console.WriteLine($"Logging payment: Amount - {amount}, Received - {receivedAmount}");
        }
    }
}
