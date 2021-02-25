using NServiceBus;

public class MyCommand : ICommand
{
    public MyEvent MyEvent { get; set; }
}