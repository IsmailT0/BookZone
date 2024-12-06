using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace BookZone.Attributes.PaymentStrategy
{
    public class PayPalPayment : IPaymentStrategy
    {

        public PayPalPayment()
        {}

        public Boolean ProcessPayment(decimal amount,decimal receivedAmount)
        {
            Console.WriteLine($"Expected payment amount is {amount}");
            if (amount == receivedAmount)
            {
                Console.WriteLine("Payment successful finished using PayPal");
                return true;
            }
            Console.WriteLine("Payment failed");
            return false;
        }
    }
}


