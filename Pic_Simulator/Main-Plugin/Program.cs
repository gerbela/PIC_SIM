
using Pic_Simulator;
public class Mainprogramm
{

    [STAThread]
    public static void Main(string[] args)
    {
        WPF_Filemanager file = new WPF_Filemanager();
        WPFController wpfController = new WPFController(file);
        wpfController.command.startUpRam();

        Console.WriteLine("Starting WPF Programm");
        var app = new App();
        var mainWindow = new MainWindow(wpfController);
        app.Run(mainWindow);
    }
}


