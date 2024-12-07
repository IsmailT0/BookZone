using System;

namespace BookZone.Attributes.PaymentStrategy
{
    public class PaymentContext
    {
        private PaymentStrategy _paymentStrategy;

        public PaymentContext(PaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public PaymentContext()
        {
            _paymentStrategy = new CreditCardPayment(); // Default strategy
        }

        // Execute payment process
        public bool ExecutePayment(decimal amount, decimal receivedAmount)
        {
            return _paymentStrategy.ExecutePayment(amount, receivedAmount);
        }


        // Set payment strategy at runtime
        public void SetPaymentStrategy(PaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }
    }
}
