using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.SqlServer.SimpleSender";
        var endpointConfiguration = new EndpointConfiguration("Samples.SqlServer.SimpleSender");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UseSerialization<NewtonsoftSerializer>()
            .Settings(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new SkipAssemblyNameForMessageTypesBinder(new[] { typeof(MyEvent)})
            });
        #region TransportConfiguration

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        var connection = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=SqlServerSimple;Integrated Security=True;Max Pool Size=100";
        transport.ConnectionString(connection);
        transport.Routing().RouteToEndpoint(typeof(MyCommand), "Samples.SqlServer.SimpleReceiver");

        #endregion

        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

        SqlHelper.EnsureDatabaseExists(connection);
        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        await SendMessages(endpointInstance);
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }

    static async Task SendMessages(IMessageSession messageSession)
    {
        Console.WriteLine("Press [c] to send a command, or [e] to publish an event. Press [Esc] to exit.");
        while (true)
        {
            var input = Console.ReadKey();
            Console.WriteLine();

            switch (input.Key)
            {
                case ConsoleKey.C:
                    await messageSession.Send(new MyCommand());
                    break;
                case ConsoleKey.E:
                    await messageSession.Publish<MyEvent>();
                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }
}

class SkipAssemblyNameForMessageTypesBinder : ISerializationBinder
{
    Type[] messageTypes;

    public SkipAssemblyNameForMessageTypesBinder(Type[] messageTypes)
    {
        this.messageTypes = messageTypes;
    }

    public Type BindToType(string assemblyName, string typeName)
    {
        return messageTypes.FirstOrDefault(messageType => messageType.FullName == typeName);
    }

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = serializedType.Assembly.FullName;
        typeName = serializedType.FullName;
    }
}