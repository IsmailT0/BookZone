using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace BookZone.Attributes.PaymentStrategy
{
    public class BankTransferPayment : PaymentStrategy
    {
        public override bool ProcessPayment(decimal amount, decimal receivedAmount)
        {
            Console.WriteLine("Processing payment through Bank Transfer...");
            return receivedAmount == amount;
        }
    }
}


