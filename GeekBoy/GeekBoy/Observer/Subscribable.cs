using System.Collections.Generic;

namespace GeekBoy.Observer
{
    public class Subscribable
    {
        private List<IObserver> _observers = new List<IObserver>();

        public void Subscribe(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void NotifyAll(NotifyData notifyData)
        {
            foreach(IObserver observer in _observers)
            {
                observer.Notify(notifyData);
            }
        }
    }
}
