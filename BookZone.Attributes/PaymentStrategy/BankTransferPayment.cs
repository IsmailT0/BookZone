using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace BookZone.Attributes.PaymentStrategy
{
    public class BankTransferPayment : IPaymentStrategy
    {
        private readonly ILogger<BankTransferPayment> _logger;

        public BankTransferPayment(ILogger<BankTransferPayment> logger)
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


