using System.Data;
using System.Windows;


namespace Pic_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
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
            Command.ResetController();
        }

        private void OneStep(object sender, RoutedEventArgs e)
        {
            if (!LST_File.loadedFile) return;
            if (LST_File.pos >= LST_File.fileSize) return;
            if (LST_File.CheckCommand(Stack) == false)
            {
                LST_File.MarkLine(Stack, CodeScroller);
                return;
            };
            int command = Fetch();
            if (!Decode(command)) return;
            LST_File.MarkLine(Stack, CodeScroller);
            //Command.Timer0();
            Result.Text = "";
            //print ram
            for (int i = 0; i < 128; i++)
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
            if ((command & 0x3F80) == 0x0780 || (command & 0x3F80) == 0x0700)
            {
                Command.ADDWF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0500 || (command & 0x3F80) == 0x0580)
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
            if ((command & 0x3F80) == 0x0180)
            {
                Command.CLRF(command & 0x7F);
            }
            if ((command & 0x3F80) == 0x0100)
            {
                Command.CLRW();
            }
            if ((command & 0x3F80) == 0x0980 || (command & 0x3F80) == 0x0900)
            {
                Command.COMF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0380 || (command & 0x3F80) == 0x0300)
            {
                Command.DECF(command & 0xFF);
            }
            if ((command & 0x3800) == 0x2000)
            {
                Command.CALL(command & 0xFF, Stack);
            }
            if ((command & 0xFFFF) == 0x0008)
            {
                bool tmpPos = Command.RETURN(Stack);
            }
            if ((command & 0x3800) == 0x2800)
            {
                Command.GOTO(command & 0x7FF, Stack);
            }
            if ((command & 0xFC00) == 0x3400)
            {
                bool tmpPos = Command.RETLW(command & 0xFF, Stack);
            }
            if ((command & 0x3F80) == 0x0B80 || (command & 0x3F80) == 0x0B00)
            {
                Command.DECFSZ(command & 0xFF, Stack);

            }
            if ((command & 0x3F80) == 0x0A80 || (command & 0x3F80) == 0x0A00)
            {
                Command.INCF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0F80 || (command & 0x3F80) == 0xF00)
            {
                Command.INCFSZ(command & 0xFF, Stack);
            }
            if ((command & 0x3F80) == 0x0480 || (command & 0x3F80) == 0x0400)
            {
                Command.IORWF(command & 0xFF);
            }

            if ((command & 0x3F80) == 0x0880 || (command & 0x3F80) == 0x0800)
            {
                Command.MOVF(command & 0xFF);
            }

            if ((command & 0xFFFF) == 0x0000)
            {
                Command.NOP();
            }

            if ((command & 0x3F80) == 0x0D80 || (command & 0x3F80) == 0x0D00)
            {
                Command.RLF(command & 0xFF);
            }

            if ((command & 0x3F80) == 0x0C80 || (command & 0x3F80) == 0x0C00)
            {
                Command.RRF(command & 0xFF);
            }

            if ((command & 0x3F80) == 0x0680 || (command & 0x3F80) == 0x0600)
            {
                Command.XORWF(command & 0xFF);
            }

            if ((command & 0x3F00) == 0x3A00)
            {
                Command.XORLW(command & 0xFF);
            }


            if ((command & 0x3C00) == 0x1000)
            {
                Command.BCF(command & 0x03FF);
            }
            if ((command & 0x3C00) == 0x1400)
            {
                Command.BSF(command & 0x03FF);
            }
            if ((command & 0x3C00) == 0x1800)
            {
                Command.BTFSC(command & 0x03FF, Stack);
            }
            if ((command & 0x3C00) == 0x1C00)
            {
                Command.BTFSS(command & 0x03FF, Stack);
            }
            if ((command & 0x3F00) == 0x0E00)
            {
                Command.SWAPF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0280 || (command & 0x3F80) == 0x0200)
            {
                Command.SUBWF(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3800)
            {
                Command.IORLW(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3C00)
            {
                Command.SUBLW(command & 0xFF);
            }
            return true;

        }

    }
}
