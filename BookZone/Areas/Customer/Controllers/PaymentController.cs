// PaymentController.cs
using BookZone.Attributes.PaymentStrategy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookZone.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PaymentController : Controller
    {
        private readonly PaymentContext _paymentContext;


        public PaymentController()
        {
            _paymentContext = new PaymentContext();
        }

        public IActionResult PlaceOrder(decimal totalAmount, decimal receivedAmount)
        {
            ViewBag.TotalAmount = totalAmount;
            ViewBag.ReceivedAmount = receivedAmount;
            return View();
        }

        [HttpPost]
        public ActionResult ProcessPayment(string paymentMethod, decimal totalAmount, decimal receivedAmount)
        {
            // Set the strategy based on user input
            switch (paymentMethod)
            {
                case "CreditCard":
                    _paymentContext.SetPaymentStrategy(new CreditCardPayment());
                    break;
                case "PayPal":
                    _paymentContext.SetPaymentStrategy(new PayPalPayment());
                    break;
                case "BankTransfer":
                    _paymentContext.SetPaymentStrategy(new BankTransferPayment());
                    break;
                default:
                    return BadRequest("Invalid payment method.");
            }

            // Process payment
            _paymentContext.ProcessPayment(totalAmount, receivedAmount);

            return View();
        }
    }
}
