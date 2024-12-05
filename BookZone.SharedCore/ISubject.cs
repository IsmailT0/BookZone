using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookZone.SharedCore.Interfaces
{
    public interface ISubject
    {
        void Register(IObserver observer);
        void Remove(IObserver observer);
        void NotifyObservers(string message);
    }
}
