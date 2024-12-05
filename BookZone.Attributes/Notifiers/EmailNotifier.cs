using BookZone.SharedCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookZone.Attributes.Notifiers
{
    public class EmailNotifier : IObserver
    {
        private readonly string _email;
        public EmailNotifier(string email)
        {
            _email = email;
        }

        public void Update(string message)
        {
            // Logic to send an email
            Console.WriteLine($"Email sent to {_email}: {message}");
        }
    }
}
