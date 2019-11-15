using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RxMultithreadVerify
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Current Thread [{Thread.CurrentThread.ManagedThreadId}]");
            Console.WriteLine("New broker.");
            var QuoteBroker = new DataEventBroker<Quote>();

            
            List<RxClient> clientList = new List<RxClient>();

            Console.WriteLine("Subscribe.");
            int clientNumber = 10;
            for (int i = 0; i < clientNumber; i++)
            {
                var client = new RxClient($"client[{i}]");
                clientList.Add(client);
                QuoteBroker.Subscribe(q => client.Update(q));
            }


            Console.WriteLine("Wait for message:");

            var timeInterval = TimeSpan.FromMilliseconds(100);

            int dataCount = 50;
            for (int i = 0; i < dataCount; i++)
            {
                //Console.WriteLine($"============= Price: {(i + 1) * 10} ==============");
                Thread.Sleep(timeInterval);
                QuoteBroker.Publish(new Quote() { Dt = DateTime.Now, Price = (i+1)*10 });
            }
            
            Console.WriteLine("Done.");
            QuoteBroker.Close();

            Console.ReadLine();
        }
    }
}
