
using System.IO;
using System.Text;
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
        List<int> commands = new List<int>();
        int pos = 0;
        bool loadedFile = false;
        
        int startPos;
        int bank = 0;
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

            if (result == true)
            {
                int counter = 0x0000;
                int pos = 1;

                foreach (string s in File.ReadLines(dialog.FileName))
                {
                    string file = "";

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
                        string command = "0x" + s.Substring(5, 4);
                        if (value == counter)
                        {
                            if (value == 0) startPos = pos;
                            commands.Add(Convert.ToInt32(command, 16));
                            file = file + s;
                            counter++;
                        }
                    }
                    TextBox textBox = new TextBox();
                    textBox.Text = file;
                    textBox.IsReadOnly = true;
                    textBox.Height = 25;
                    Stack.Children.Add(textBox);
                    pos++;

                }
                //print commands
                //foreach (int i in commands) Result.Text = Result.Text + i + "\n";
                loadedFile = true;
                MarkLine();
            }
        }

        private void OneStep(object sender, RoutedEventArgs e)
        {
            MarkLine();
            int command = Fetch();
            Decode(command);
            Result.Text = "";
            //print ram
            for(int i = 0; i < 128; i++)
            {
                Result.Text = Result.Text + " " + Command.ram[bank, i];
            }
            Result.Text = Result.Text + "\n" + "W-Register: " + Command.wReg;
        }

        private int Fetch()
        {
            int programCounter = Command.ram[bank, 2];
            int command = commands[programCounter];
            programCounter++;
            Command.ram[bank, 2] = programCounter;
            return command;
        }

        private void Decode(int command)
        {
            if ((command & 0x3F00) == 0x3000)
            {
                Command.MOVLW(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0080)
            {
                Command.MovWF(command & 0x7F);
            }
            if((command & 0x3F80) == 0x0780 || (command & 0x3F80) == 0x0700)
            {
                Command.ADDWF(command & 0xFF);
            }
            if((command & 0x3F80) == 0x3780 || (command & 0x3F80) == 0x0700)
            {
                Command.ANDWF(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3E00)
            {
                Command.ADDLW(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3900)
            {
                Command.ANDLW(command & 0xFF);
            }
            if(( command & 0x3F80) == 0x0180)
            {
                Command.CLRF(command & 0x7F);
            }
            if((command & 0x3F80) == 0x0100)
            {
                Command.CLRW();
            }
            if((command & 0x3F80) == 0x0980 || (command & 0x3F80) == 0x0900)
            {
                Command.COMF(command & 0xFF);
            }
            if((command & 0x3F80) == 0x0380 || (command & 0x3F80) == 0x0300)
            {
                Command.DECF(command & 0xFF);
            }

        }
        private void MarkLine()
        {
            if (!loadedFile) return;
            if (pos == 0)
            {
                pos = startPos - 2;
                CodeScroller.ScrollToVerticalOffset(CodeScroller.VerticalOffset + 25*(startPos-4));
            }
            TextBox text = (TextBox)Stack.Children[pos];
            text.Background = Brushes.White;
            pos++;
            if (Stack.Children.Count <= pos)
            {
                pos = startPos - 2;
                CodeScroller.ScrollToVerticalOffset(0);
            }
            text = (TextBox)Stack.Children[pos];
            text.Background = Brushes.OrangeRed;
            if (pos != startPos-1) CodeScroller.ScrollToVerticalOffset(CodeScroller.VerticalOffset + 25);
        }
    }
}
