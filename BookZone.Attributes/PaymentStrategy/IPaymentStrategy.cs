using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookZone.Attributes.PaymentStrategy
{
    public interface IPaymentStrategy
    {
        Boolean ProcessPayment(decimal amount,decimal recievedAmount);
    }
}
