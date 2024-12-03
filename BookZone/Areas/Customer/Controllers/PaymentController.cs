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
        private readonly ILogger<PaymentController> _logger;
        private readonly CreditCardPayment _creditCardPayment;
        private readonly PayPalPayment _payPalPayment;
        private readonly BankTransferPayment _bankTransferPayment;

        public PaymentController(
            ILogger<PaymentController> logger,
            CreditCardPayment creditCardPayment,
            PayPalPayment payPalPayment,
            BankTransferPayment bankTransferPayment)
        {
            _logger = logger;
            _creditCardPayment = creditCardPayment;
            _payPalPayment = payPalPayment;
            _bankTransferPayment = bankTransferPayment;
            _paymentContext = new PaymentContext(_creditCardPayment);
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
                    _paymentContext.SetPaymentStrategy(_creditCardPayment);
                    break;
                case "PayPal":
                    _paymentContext.SetPaymentStrategy(_payPalPayment);
                    break;
                case "BankTransfer":
                    _paymentContext.SetPaymentStrategy(_bankTransferPayment);
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
