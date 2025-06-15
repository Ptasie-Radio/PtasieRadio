using Uno.UI.Runtime.Skia;

namespace PtasieRadio;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Exception ex = (Exception)e.ExceptionObject;
            string logPath = Path.Combine(AppContext.BaseDirectory, "error.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] Unhandled exception: {ex}\n\n");
        };
    
        var host = SkiaHostBuilder.Create()
            .App(() => new App())
            .UseWindows()
            .Build();
    
        host.Run();
    }
    
}