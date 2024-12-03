using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookZone.Attributes.PaymentStrategy
{
    public class PaymentContext
    {
        private IPaymentStrategy _paymentStrategy;
        public PaymentContext(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }
        public Boolean ProcessPayment(decimal amount,decimal receivedAmount)
        {
            return _paymentStrategy.ProcessPayment(amount,receivedAmount);
        }

        public void SetPaymentStrategy(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }
    }
}
