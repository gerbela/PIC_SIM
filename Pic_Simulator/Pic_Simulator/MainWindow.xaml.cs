
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
        public static List<int> commands = new List<int>();
        int pos = 0;
        bool loadedFile = false;
        
        int startPos;
        int fileSize;
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
                            if (value == 0) startPos = pos - 1;
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
                    fileSize++;

                }
                //print commands
                //foreach (int i in commands) Result.Text = Result.Text + i + "\n";
                loadedFile = true;
                Setup();
            }
        }

        private void OneStep(object sender, RoutedEventArgs e)
        {
            if (!loadedFile) return;
            if (pos >= fileSize) return;
            if (CheckCommand() == false)
            {
                MarkLine();
                return;
            };
            int command = Fetch();
            if(!Decode(command)) return;
            MarkLine();
            Result.Text = "";
            //print ram
            /*for(int i = 0; i < 128; i++)
            {
                Result.Text = Result.Text + " " + Command.ram[bank, i];
            }
            Result.Text = Result.Text + "\n" + "W-Register: " + Command.wReg;*/
        }

        private int Fetch()
        {
            int programCounter = Command.ram[bank, 2];
            int command = commands[programCounter];
            programCounter++;
            Command.ram[bank, 2] = programCounter;
            return command;
        }

        private bool Decode(int command)
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
            if((command & 0x3800) == 0x2000)
            {
                Command.Call(command & 0xFF);
                ClearMarker();
                pos = FindFilePos(command & 0xFF) -3;//(command & 0xFF) + startPos;
                MarkLine();
            }
            if((command & 0xFFFF) == 0x0008)
            {
                int tmpPos = Command.Return();
                if (tmpPos != -1)
                {
                    ClearMarker();
                    pos = FindFilePos(tmpPos) - 2;
                    MarkLine();
                }
                else
                {
                    pos++;
                    return false;
                }
            }
            return true;

        }
        private Boolean CheckCommand()
        {
            TextBox t = (TextBox)Stack.Children[pos];
            if (t.Text.StartsWith(" ")) return false;
            int commandPos = Convert.ToInt32(t.Text.Substring(20, 5));
            if (commandPos -1 == pos) return true;
            return false;
        }

        private void ClearMarker()
        {
            TextBox text = (TextBox)Stack.Children[pos];
            text.Background = Brushes.White;
        }

        private void Setup()
        {
            if (!loadedFile) return;
            if (pos == 0)
            {
                pos = startPos;
                TextBox t = (TextBox)Stack.Children[pos];
                t.Background = Brushes.OrangeRed;
                CodeScroller.ScrollToVerticalOffset(CodeScroller.VerticalOffset + 25 * (startPos - 4));
                return;
            }
        }

        private int FindFilePos(int programPos)
        {
            foreach(TextBox t in Stack.Children)
            {
                if (t.Text.StartsWith(" ")) continue;
                int commandPos = Convert.ToInt32(t.Text.Substring(0, 4),16);
                if (commandPos == programPos)
                {
                    int tmp = Convert.ToInt32(t.Text.Substring(20, 5));
                    return tmp;
                }
            }
            return -1;
        }
        private void MarkLine()
        {
            if (!loadedFile) return;
            if (pos > fileSize) return;
            if (pos == 0)
            {
                pos = startPos;
                TextBox t = (TextBox)Stack.Children[pos];
                t.Background = Brushes.OrangeRed;
                CodeScroller.ScrollToVerticalOffset(CodeScroller.VerticalOffset + 25 * (startPos - 4));
                return;
            }
            TextBox text = (TextBox)Stack.Children[pos];
            text.Background = Brushes.White;
            pos++;
            text = (TextBox)Stack.Children[pos];
            text.Background = Brushes.OrangeRed;
            CodeScroller.ScrollToVerticalOffset(startPos + 25 *(pos -4));
        }
    }
}
