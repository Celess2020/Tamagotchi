using System;
using Avalonia;

namespace Tamagotchi;

class Program
{
    // Punto de entrada. No tocar salvo que sepas lo que estás haciendo :)
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
