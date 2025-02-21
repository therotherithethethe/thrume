var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello", async () =>
{
    Console.WriteLine($"Start: {DateTime.Now:HH:mm:ss.fff}, Thread: {Thread.CurrentThread.ManagedThreadId}");
    await Task.Delay(TimeSpan.FromSeconds(10));
    Console.WriteLine($"End: {DateTime.Now:HH:mm:ss.fff}, Thread: {Thread.CurrentThread.ManagedThreadId}");
    return "Hello, World!";
});

app.Run();