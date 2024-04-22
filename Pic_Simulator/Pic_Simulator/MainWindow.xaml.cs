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
                Command.MOVWF(command & 0x7F);
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
                Command.CALL(command & 0xFF,Stack);
            }
            if((command & 0xFFFF) == 0x0008)
            {
                bool tmpPos = Command.RETURN(Stack);
            }
            if((command & 0x3800) == 0x2800)
            {
                Command.GOTO(command & 0x7F, Stack);
            }
            if ((command & 0xFC00) == 0x3400)
            {
                bool tmpPos = Command.RETLW(command & 0xFF, Stack);
            }
            if ((command & 0x3F80) == 0x0B80 || (command & 0x3F80) == 0x0B00)
            {
                Command.DECFSZ(command & 0xFF, Stack); 
            
            }
            return true;

        }
    }
}
