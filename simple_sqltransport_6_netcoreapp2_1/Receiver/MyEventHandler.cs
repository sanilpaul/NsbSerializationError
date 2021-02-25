using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MyEventHandler : IHandleMessages<MyEvent>
{
    static ILog log = LogManager.GetLogger<MyEventHandler>();

    public async Task Handle(MyEvent eventMessage, IMessageHandlerContext context)
    {
        log.Info($"Hello from {nameof(MyEventHandler)}");
        await context.SendLocal(new MyCommand { MyEvent = eventMessage });
    }
}