using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace BookZone.Attributes.PaymentStrategy
{
    public class CreditCardPayment : IPaymentStrategy
    {
        private readonly ILogger<CreditCardPayment> _logger;

        public CreditCardPayment(ILogger<CreditCardPayment> logger)
        {
            _logger = logger;
        }

        public Boolean ProcessPayment(decimal amount,decimal receivedAmount)
        {
            _logger.LogInformation($"Expected payment amount is {amount}");
            if(amount == receivedAmount)
            {
                return true;
            }
            return false;
        }
    }
}

