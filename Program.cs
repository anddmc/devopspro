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
	        		            ch_in.QueueDeclare(queue: "bakery.wafer.order",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
	        		            ch_out.QueueDeclare(queue: "bakery.wafer.done",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
	        		ch_in.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
	        		Console.Write(System.Environment.MachineName + ":-- [*] Waiting for waffles back orders (backing time = " + secs + ")");

	        		var consumer = new EventingBasicConsumer(ch_in);
		            consumer.Received += (model, ea) =>
		            {
		                var body = ea.Body;
		                var message = Encoding.UTF8.GetString(body);
		                JToken token = JObject.Parse(message);
		                var flavour =  (string)token.SelectToken("flavour");
		                Console.WriteLine(" [x] Received {0}", message);
		
		                Console.WriteLine(System.Environment.MachineName + ":-- [.] Start backing a " + flavour + " wafer");
		                Thread.Sleep(secs * 1000);
		                Console.WriteLine(System.Environment.MachineName + ":-- [.] Start backing a " + flavour + " done");
		
						ch_in.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
						var properties = ch_out.CreateBasicProperties();
            			properties.SetPersistent(true);

			            ch_out.BasicPublish(exchange: "",
			                                 routingKey: "bakery.wafer.done",
			                                 basicProperties: properties,
			                                 body: body);
					            };


		            ch_in.BasicConsume(queue: "bakery.wafer.order",
		                                 noAck: false,
		                                 consumer: consumer);
		
		            Console.WriteLine(" Press [enter] to exit.");
		            Console.ReadLine();
	        	}
	        }			
			
			Console.Write("EXIT");
//			Console.ReadKey(true);
		}
	}
}