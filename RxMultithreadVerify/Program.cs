using System;
using System.Collections.Generic;
using System.IO;
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

            int clientNumber = 5000;
            int msec = 1000;
            int dataCount = 60;


            Console.WriteLine($"Current Thread [{Thread.CurrentThread.ManagedThreadId}]");
            Console.WriteLine("New broker.");
            var QuoteBroker = new DataEventBroker<Quote>();


            List<RxClient> clientList = new List<RxClient>();


            var rxInspecter = new RxInspecter();



            Console.WriteLine("Subscribe.");
            

            for (int i = 0; i < clientNumber; i++)
            {
                var client = new RxClient(msec,$"client[{i}]",rxInspecter.InspectBroker,rxInspecter.InspectStatBroker);
                clientList.Add(client);
                QuoteBroker.Subscribe(async q => await client.Update(q));
            }


            Console.WriteLine("Wait for message:");

            
            var timeInterval = TimeSpan.FromMilliseconds(msec);

            
            for (int i = 0; i < dataCount; i++)
            {
                //Console.WriteLine($"============= Price: {(i + 1) * 10} ==============");
                Thread.Sleep(timeInterval);
                QuoteBroker.Publish(new Quote() {Dt = DateTime.Now, Price = (i + 1) * 10});
            }

            Console.WriteLine("Done.");
            QuoteBroker.Close();


            Thread.Sleep(10000);

            Console.WriteLine(rxInspecter.TotalPriceError);
            Console.WriteLine(rxInspecter.TotalTimeDelayExceedTol);

            rxInspecter.InspectStatBroker.Close();
            var avg = rxInspecter.TimeDelay.Average();
            Console.WriteLine(avg.ToString());
            
            using (StreamWriter sr = new StreamWriter($"msec_{msec}_pos_{clientNumber}_data_{dataCount}.txt"))
            {

                //for (int i = 0; i < rxInspecter.TimeDelay.Count; i++)
                //{
                //    Console.WriteLine(rxInspecter.TimeDelay[i]);
                //    sr.WriteLine(rxInspecter.TimeDelay[i].ToString());
                //}

                foreach (var item in rxInspecter.TimeDelay)
                {
                    sr.WriteLine(item.ToString());
                }
            }



            Console.ReadLine();
        }
    }
}
