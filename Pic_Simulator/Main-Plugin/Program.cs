
using Pic_Simulator;
public class Mainprogramm
{

    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var app = new App();
        //app.InitializeComponent();
        var mainWindow = new MainWindow();
        app.Run(mainWindow);
    }
}


