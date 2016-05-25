using System;
using System.Text;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Threading;

namespace WafflesMinion
{
    class Program
    {
        public static void Main(string[] args)
        { 
            var secs = 2;

            Console.WriteLine(System.Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"));
            Console.WriteLine(System.Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD"));
            Thread.Sleep(5000);
            
            var factory = new ConnectionFactory() { HostName = "rabbitmq.cloud66.local", 
                UserName = System.Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"),
                Password = System.Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") 
            };
            using (var connection = factory.CreateConnection())
            {
                using (var ch_in = connection.CreateModel())
                using (var ch_out = connection.CreateModel())
                {
                                ch_in.QueueDeclare(queue: "bakery.waffle.order",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
                                ch_out.QueueDeclare(queue: "bakery.sweet.done",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
                    ch_in.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.Write(System.Environment.MachineName + ":-- [*] Waiting for waffle back orders (backing time = " + secs + ")");

                    var consumer = new EventingBasicConsumer(ch_in);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
//                        dynamic data = JValue.Parse(message);
                        JToken token = JObject.Parse(message);
                        var flavour =  (string)token.SelectToken("flavour");
                        Console.WriteLine(" [x] Received {0}", message);
        
                        Console.WriteLine(System.Environment.MachineName + ":-- [.] Start backing a " + flavour + " waffle");
                        Thread.Sleep(secs * 1000);
                        Console.WriteLine(System.Environment.MachineName + ":-- [.] Start backing a " + flavour + " done");
        
                        ch_in.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//                        ch_out..sendToQueue('bakery.sweet.done', new Buffer(msg.content), {persistent: true});
                        var properties = ch_out.CreateBasicProperties();
                        properties.SetPersistent(true);

                        ch_out.BasicPublish(exchange: "",
                                             routingKey: "bakery.sweet.done",
                                             basicProperties: properties,
                                             body: body);
                                };


                    ch_in.BasicConsume(queue: "bakery.waffle.order",
                                         noAck: false,
                                         consumer: consumer);
        
                }
            }            
            
//            Console.Write("Press any key to continue . . . ");
//            Console.ReadKey(true);
        }
    }
}
