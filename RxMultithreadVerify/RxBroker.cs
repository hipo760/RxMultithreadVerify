using System;
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


    public class RxClient
    {
        public string Name { get; set; }
        private int _currentPrice = 0;
        private DateTime dt;

        public RxClient(string name)
        {
            Name = name;
            dt = DateTime.Now;
        }
        public Task Update(Quote quote)
        {
            return Task.Run(() =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(10));
                });
                

                //if (currentPrice > quote.Price)
                //{
                //    int id = Thread.CurrentThread.ManagedThreadId;
                //    Task.Run(() =>
                //    {
                //        Console.WriteLine("Price order error");
                //        Console.WriteLine($"[{id}] " +
                //                          $"Client {Name} " +
                //                          $"Previous price {currentPrice} " +
                //                          $"received quote: {quote} at {DateTime.Now:hh:mm:ss.ffffff}.");
                //    });

                //}

                var delay = (quote.Dt - dt).Duration() - TimeSpan.FromMilliseconds(1000);


                if ( delay > TimeSpan.FromMilliseconds(1))
                {
                    int id = Thread.CurrentThread.ManagedThreadId;

                    Task.Run(()=>
                    {
                        Console.WriteLine($"Time delay.{(delay).TotalMilliseconds}");
                        Console.WriteLine($"[{id}] " +
                                          $"Client {Name} " +
                                          $"Previous dt {dt:hh:mm:ss.ffffff} " +
                                          $"received quote: {quote} at {DateTime.Now:hh:mm:ss.ffffff}.");
                    });
                }

                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] " +
                                  $"Client {Name} " +
                                  $"received quote: {quote} at {DateTime.Now:hh:mm:ss.ffffff}.");

                _currentPrice = quote.Price;
                dt = quote.Dt;
            });
        }
    }

}