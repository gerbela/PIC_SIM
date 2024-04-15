
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Pic_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int[,] ram = new int[2,128];
        List<int> commands = new List<int>();
        int pos = 0;
        bool loadedFile = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void LoadFile(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".lst";
            dialog.Filter = "Text documents (.lst)|*.lst";
            bool? result = dialog.ShowDialog();

            if(result == true) 
            {
                int counter = 0x0000;
                bool reachedCommands = false;
                int pos = 1;

                foreach (string s in File.ReadLines(dialog.FileName)) 
                {
                    string file = "";
                    if (!s.StartsWith("0000") && reachedCommands == false)
                    {
                        string tmp = "        " + s;
                        file = file + tmp;
                    }
                    else
                    {
                        reachedCommands = true;

                        string firstFour = s.Substring(0, 4);
                        if (s.Substring(0, 4) == "    ")
                        {
                            string tmp = "        " + s;
                            file = file + tmp;
                        }
                        else
                        {
                            firstFour = "0x" + firstFour;
                            int value = Convert.ToInt32(firstFour, 16);
                            string command = "0x" + s.Substring(5,4);
                            if (value == counter)   
                            {
                                commands.Add(Convert.ToInt32(command,16));
                                file = file + s;
                                counter++;
                            }
                        }
                    }
                    TextBox textBox = new TextBox();
                    textBox.Text = file;
                    textBox.IsReadOnly = true;
                    textBox.Height = 25;
                    Stack.Children.Add(textBox);
                    pos++;

                }
                foreach (int i in commands) Result.Text = Result.Text + i + "\n";
                loadedFile = true;
            }
        }

        private void OneStep(object sender, RoutedEventArgs e)
        {
            MarkLine();
            Fetch();
        }

        private void Fetch()
        {
            int programCounter = ram[0,2];
            int command = ram[0, programCounter];
            programCounter++;
            ram[0, 2] = programCounter;
        }
        private void MarkLine()
        {
            if (!loadedFile) return;
            TextBox text = (TextBox)Stack.Children[pos];
            text.Background = Brushes.White;
            pos++;
            if (Stack.Children.Count <= pos)
            {
                pos = 0;
                CodeScroller.ScrollToVerticalOffset(0);
            }
            text = (TextBox)Stack.Children[pos];
            text.Background = Brushes.OrangeRed;
            if (pos != 0) CodeScroller.ScrollToVerticalOffset(CodeScroller.VerticalOffset + 25);
        }
    }
}
