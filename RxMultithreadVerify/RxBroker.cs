using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RxMultithreadVerify
{

    public class Quote
    {
        public DateTime Dt { get; set; }
        public int Price { get; set; }

        public override string ToString()
        {
            return $"Time: {Dt:hh:mm:ss.ffffff}, Price: {Price}";
        }
    }
    
    
    public class RxBroker
    {
        public DataEventBroker<Quote> QuoteBroker;
        
        
        public RxBroker()
        {
            QuoteBroker = new DataEventBroker<Quote>();
        }
    }

    public class InspectEvent
    {
        public int PriceErrorCount { get; set; }
        public int TimeDelayCount { get; set; }
    }


    public class RxInspecter
    {
        public DataEventBroker<InspectEvent> InspectBroker { get; set; }
        public DataEventBroker<double> InspectStatBroker { get; set; }
        public int TotalPriceError { get; set; } = 0;
        public int TotalTimeDelayExceedTol { get; set; } = 0;
        public List<double> TimeDelay { get; set; }

        public RxInspecter()
        {
            InspectBroker = new DataEventBroker<InspectEvent>();
            InspectStatBroker = new DataEventBroker<double>();
            TimeDelay = new List<double>();
            InspectBroker.Subscribe(i =>
            {
                Task.Run(() =>
                {
                    TotalPriceError += i.PriceErrorCount;
                    TotalTimeDelayExceedTol += i.TimeDelayCount;
                });
            });
            InspectStatBroker.Subscribe(s => Task.Run(() => TimeDelay.Add(s)));
        }
    }



    public class RxClient
    {
        public string Name { get; set; }
        private int _currentPrice = 0;
        private DateTime dt;
        private DataEventBroker<InspectEvent> _inspectBroker;
        private DataEventBroker<double> _inspectStatBroker;
        private int _interval;


        

        public RxClient(int interval, string name, DataEventBroker<InspectEvent> inspectBroker, DataEventBroker<double> inspectStatBroker)
        {
            Name = name;
            dt = DateTime.Now;
            _inspectBroker = inspectBroker;
            _inspectStatBroker = inspectStatBroker;
            _interval = interval;
        }

        public Task Update(Quote quote)
        {
            return Task.Run(() =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                });


                if (_currentPrice > quote.Price)
                {
                    int id = Thread.CurrentThread.ManagedThreadId;
                    Task.Run(() =>
                    {
                        //Console.WriteLine("Price order error");
                        //Console.WriteLine($"[{id}] " +
                        //                  $"Client {Name} " +
                        //                  $"Previous price {_currentPrice} " +
                        //                  $"received quote: {quote} at {DateTime.Now:hh:mm:ss.ffffff}.");
                        Task.Run(() =>
                        {
                            _inspectBroker.Publish(new InspectEvent() { PriceErrorCount = 1, TimeDelayCount = 0 });
                        });
                    });
                }

                var delay = (quote.Dt - dt) - TimeSpan.FromMilliseconds(_interval);


                if ( delay > TimeSpan.FromMilliseconds(500))
                {
                    int id = Thread.CurrentThread.ManagedThreadId;

                    Task.Run(()=>
                    {
                        //Console.WriteLine($"Time delay.{(delay).TotalMilliseconds}");
                        //Console.WriteLine($"[{id}] " +
                        //                  $"Client {Name} " +
                        //                  $"Previous dt {dt:hh:mm:ss.ffffff} " +
                        //                  $"received quote: {quote} at {DateTime.Now:hh:mm:ss.ffffff}.");
                        Task.Run(() =>
                        {
                            _inspectBroker.Publish(new InspectEvent() { PriceErrorCount = 0, TimeDelayCount = 1 });
                        });
                    });
                }
                //Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] " +
                //                  $"Client {Name} " +
                //                  $"received quote: {quote} at {DateTime.Now:hh:mm:ss.ffffff}.");
                Task.Run(() =>
                {
                    //Console.WriteLine(delay.TotalMilliseconds);
                    _inspectStatBroker.Publish((delay).TotalMilliseconds);
                });
                _currentPrice = quote.Price;
                dt = quote.Dt;
            });
        }
    }

}