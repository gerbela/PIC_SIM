using System.Windows;

namespace Pic_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<int> commands = new List<int>();
        int bank = 0;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void LoadFile(object sender, RoutedEventArgs e)
        {
            LST_File.LoadFile(Stack, CodeScroller);
        }

        private void OneStep(object sender, RoutedEventArgs e)
        {
            if (!LST_File.loadedFile) return;
            if (LST_File.pos >= LST_File.fileSize) return;
            if (LST_File.CheckCommand(Stack) == false)
            {
                LST_File.MarkLine(Stack,CodeScroller);
                return;
            };
            int command = Fetch();
            if(!Decode(command)) return;
            LST_File.MarkLine(Stack, CodeScroller);
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
                LST_File.ClearMarker(Stack);
                LST_File.pos = LST_File.FindFilePos(Stack, command & 0xFF) -3;//(command & 0xFF) + startPos;
                LST_File.MarkLine(Stack, CodeScroller);
            }
            if((command & 0xFFFF) == 0x0008)
            {
                int tmpPos = Command.Return();
                if (tmpPos != -1)
                {
                    LST_File.ClearMarker(Stack);
                    LST_File.pos = LST_File.FindFilePos(Stack,tmpPos) - 2;
                    LST_File.MarkLine(Stack, CodeScroller);
                }
                else
                {
                    LST_File.pos++;
                    return false;
                }
            }
            if((command & 0x3800) == 0x2800)
            {
                LST_File.ClearMarker(Stack);
                Command.GoTo(command & 0x7F);
                LST_File.pos = LST_File.FindFilePos(Stack, command & 0x7F) -3;
                LST_File.MarkLine(Stack, CodeScroller);
            }
            return true;

        }
    }
}
