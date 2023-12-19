// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ;
using System.Runtime.CompilerServices;
using RabbitMQ.Client.Events;
using System.Text;

const string QueueName = "ClientQueue";
const string RoutingKey = "hello.world";
const string Broker = "jskdev.local";

var connectionfactory = new ConnectionFactory()
{
    HostName = Broker,
    UserName = "consumer",
    Password = "P@ssw0rd",
};
Console.WriteLine("Creating connection...");
using var connection = connectionfactory.CreateConnection();

if (!connection.IsOpen)
{
    Console.WriteLine("Connection failed");
    Console.WriteLine("Reason:" + connection.CloseReason.Cause.ToString());
    System.Environment.Exit(0);
}

using IModel model = connection.CreateModel();
model.QueueDeclare(QueueName,autoDelete:true);
model.QueueBind(QueueName, "amq.topic",RoutingKey);

var consumer = new EventingBasicConsumer(model);

consumer.Received += (sender, args) =>
{
    string payload = Encoding.UTF8.GetString(args.Body.ToArray());
    if (payload.Contains("bad word"))
    {
        Console.WriteLine($"{args.DeliveryTag} is Rejected...");
        model.BasicReject(args.DeliveryTag, true);
    }
    else
    {
        Console.WriteLine("-----Message");
        Console.WriteLine("DeliveryTag: " + args.DeliveryTag);
        Console.WriteLine("Exchange: " + args.Exchange);
        Console.WriteLine("Routing key: " + args.RoutingKey);
        Console.WriteLine("Body: " + payload);
        Console.WriteLine("-----Message");

        model.BasicAck(args.DeliveryTag, false);
    }
};

model.BasicConsume(QueueName,false,consumer);

Console.WriteLine("Connected...");
Console.ReadLine();

model.Close();
connection.Close();
Console.WriteLine("Disonnected...");