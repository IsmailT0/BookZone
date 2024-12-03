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
        private readonly ILogger<PayPalPayment> _logger;

        public PayPalPayment(ILogger<PayPalPayment> logger)
        {
            _logger = logger;
        }

        public Boolean ProcessPayment(decimal amount,decimal receivedAmount)
        {
            _logger.LogInformation($"Expected payment amount is {amount}");
            if (amount == receivedAmount)
            {
                return true;
            }
            return false;
        }
    }
}


