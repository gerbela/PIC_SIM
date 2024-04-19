﻿
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
        int[,] ram = new int[2, 128];
        List<int> commands = new List<int>();
        int pos = 0;
        bool loadedFile = false;
        int wReg = 0;
        
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
                Result.Text = Result.Text + " " + ram[bank, i];
            }
            Result.Text = Result.Text + "\n" + "W-Register: " + wReg;
        }

        private int Fetch()
        {
            int programCounter = ram[bank, 2];
            int command = commands[programCounter];
            programCounter++;
            ram[bank, 2] = programCounter;
            return command;
        }

        private void Decode(int command)
        {
            if ((command & 0x3F00) == 0x3000)
            {
                MOVLW(command & 0xFF);
            }
            if ((command & 0xFFA0) == 0x00A0)
            {
                MovWF(command & Convert.ToInt32("7F", 16));
            }
            if((command & 0x3F80) == 0x0780 || (command & 0x3F80) == 0x0700)
            {
                ADDWF(command & 0xFF);
            }
            if((command & 0x3F80) == 0x3780 || (command & 0x3F80) == 0x0700)
            {
                ANDWF(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3E00)
            {
                ADDLW(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3900)
            {
                ANDLW(command & 0xFF);
            }

        }
        private void ANDWF(int address)
        {
            int result = wReg & ram[bank, address & 0x7F];
            if (result == 0)
            {
                ram[bank, 3] = ram[bank, 3] | 0b00000100;
            }
            if((address & 0x0080) == 0x0080)
            {
                ram[bank, address & 0x007F] = result;
            }
            else
            {
                wReg = result;
            }
        }

        private void ADDWF(int address)
        {
            int result = ADD(ram[bank, address & 0x007F]);
            if ((address & 0x0080) == 0x0080)
            {
                ram[bank, address & 0x007F] = result;
            }
            else
            {
                wReg = result;
            }
        }
        private int ADD(int value)
        {
            if(value + wReg > 127) // wann wird der gesetzt?
            {
                ram[bank, 3] = ram[bank, 3] | 0b00000010; //Half Carryflag
            }
            if(value + wReg > 256)
            {
                ram[bank, 3] = ram[bank, 3] | 0b00000001; // Carryflag
            }
            if((value + wReg) % 256 == 0)
            {
                ram[bank, 3] = ram[bank, 3] | 0b00000100; // Zeroflag
            }
            return (value + wReg) % 256; // Wird carry immer aktiv auf 0 gesetzt?
        }
        private void MOVLW(int literal)
        {
            wReg = literal;
        }

        private void MovWF(int storageLocation)
        {
            ram[bank, storageLocation] = wReg;
        }
        private void ADDLW(int literal)
        {
            int result = ADD(literal);
            wReg = result; 
        }

        private void ANDLW(int literal)
        {
            wReg = literal & wReg; 
            if(wReg == 0) {
                ram[0, 3] = ram[0, 3] | 0b00000100; //Zeroflag
            }
            else
            {
                ram[0,3] = ram[0, 3] | 0b00000000; //Zeroflag
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
