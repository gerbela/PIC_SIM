using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;


namespace Pic_Simulator
{

 
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static List<int> commands = new List<int>();
        DataTable tableRB = new DataTable();
        DataTable tableRA = new DataTable();
        DataTable tableSTR = new DataTable();
        DataTable tableIntCon = new DataTable();
        DataTable tableOption = new DataTable();
        DataTable tableStack = new DataTable();
        double runTime = 0;
        private DispatcherTimer timer;
        bool run = false;
        public WPFController wpfController;
        LST_File file = new LST_File();

        public MainWindow()
        {
            wpfController = new WPFController(file);
            InitializeComponent();
            wpfController.command.startUpRam();  
            PrintRam();
            PrintRaRb();
            PrintSTR();
            PrintOption();
            PrintINTCON();
            PrintStack();
        }

        private void refreshUI()
        {
            PrintRam();
            RefreshRegister(tableRA, RAGrid, wpfController.command.bank, 5, 0);
            RefreshRegister(tableRB, RBGrid, wpfController.command.bank, 6, 0);
            UpdateTrisValues();
            RefreshRegister(tableSTR, STRGrid, wpfController.command.bank, 3);
            RefreshRegister(tableIntCon, INTCONGrid, 0, 11);
            RefreshRegister(tableOption, OptionGrid, 1, 1);
            refreshStack();
            lightLEDs();
        }
        
        private void LoadFile(object sender, RoutedEventArgs e)
        {
            bool loading_sucess = LST_File.LoadFile(Stack, CodeScroller);
            if (!loading_sucess) return;
            wpfController.ResetControllerRoutine(Stack);
            refreshUI();
            resetLEDs();


        }
        void selectedCellsChangedRA(object sender, RoutedEventArgs e)
        {
            int rowIndex = RAGrid.Items.IndexOf(RAGrid.CurrentItem);
            int colIndex = RAGrid.CurrentCell.Column.DisplayIndex;
            string storageVal = (string)tableRA.Rows[rowIndex][colIndex];
            int cellValue = Convert.ToInt32(storageVal);  
            tableRA.Rows[rowIndex][colIndex] = (cellValue == 0) ? 1 : 0;
            int newBit = 0;
            
            if (cellValue == 0)
            {
               newBit = 1;
            }           
            int ramBit = Command.SetSelectedBit(wpfController.command.ram[wpfController.command.bank, 5], Math.Abs(colIndex - 7), newBit);
            wpfController.command.ram[wpfController.command.bank, 5] = ramBit;
            refreshUI();
        }

        private void selectedCellsChangedSTR(object sender, RoutedEventArgs e) {
            int rowIndex = STRGrid.Items.IndexOf(STRGrid.CurrentItem);
            int colIndex = STRGrid.CurrentCell.Column.DisplayIndex;
            int cellValue = (int)tableSTR.Rows[rowIndex][colIndex];  
            tableSTR.Rows[rowIndex][colIndex] = (cellValue == 0) ? 1 : 0;
            int newBit = 0;

            if (cellValue == 0)
            {
                newBit = 1;
            }
            int ramBit = Command.SetSelectedBit(wpfController.command.ram[wpfController.command.bank, 3], Math.Abs(colIndex - 7), newBit);
            wpfController.command.ram[wpfController.command.bank, 3] = ramBit;
            refreshUI();

        }

        private void selectedCellsChangedINTCON(object sender, RoutedEventArgs e)
        {
            int rowIndex = INTCONGrid.Items.IndexOf(INTCONGrid.CurrentItem);
            int colIndex = INTCONGrid.CurrentCell.Column.DisplayIndex;
            int cellValue = (int)tableIntCon.Rows[rowIndex][colIndex];
            tableIntCon.Rows[rowIndex][colIndex] = (cellValue == 0) ? 1 : 0;
            int newBit = 0;

            if (cellValue == 0)
            {
                newBit = 1;
            }
            int ramBit = Command.SetSelectedBit(wpfController.command.ram[0, 11], Math.Abs(colIndex - 7), newBit);
            wpfController.command.ram[0, 11] = ramBit;
            refreshUI();
        }

        private void selectedCellsChangedOption(object sender, RoutedEventArgs e)
        {
            int rowIndex = OptionGrid.Items.IndexOf(OptionGrid.CurrentItem);
            int colIndex = OptionGrid.CurrentCell.Column.DisplayIndex;
            int cellValue = (int)tableOption.Rows[rowIndex][colIndex];
            tableOption.Rows[rowIndex][colIndex] = (cellValue == 0) ? 1 : 0;
            int newBit = 0;

            if (cellValue == 0)
            {
                newBit = 1;
            }
            int ramBit = Command.SetSelectedBit(wpfController.command.ram[1, 1], Math.Abs(colIndex - 7), newBit);
            wpfController.command.ram[1, 1] = ramBit;
            refreshUI();
        }

        void selectedCellsChangedRB(object sender, RoutedEventArgs e)
        {
            int rowIndex = RBGrid.Items.IndexOf(RBGrid.CurrentItem);
            int colIndex = RBGrid.CurrentCell.Column.DisplayIndex;
            int cellValue = Convert.ToInt32((string)tableRB.Rows[rowIndex][colIndex]);
            tableRB.Rows[rowIndex][colIndex] = (cellValue == 0) ? 1 : 0;
            int newBit = 0;

            if (cellValue == 0)
            {
                newBit = 1;
            }
            int ramBit = Command.SetSelectedBit(wpfController.command.ram[wpfController.command.bank, 6], Math.Abs(colIndex - 7), newBit);
            wpfController.command.ram[wpfController.command.bank, 6] = ramBit;
            refreshUI();
        }

        private void RefreshRegister(DataTable table, DataGrid grid, int bank, int address, int rowIndex = 0)
        {
            for (int i = 7; i >= 0; i--)
            {
                table.Rows[rowIndex][i] = Command.GetSelectedBit(wpfController.command.ram[bank, address], Math.Abs(i - 7));
            }
            grid.ItemsSource = table.DefaultView;
        }

        private void UpdateTrisValues()
        {
            for (int i = 7; i >= 0; i--)
            {
                // Update TRIS values for RA
                int trisA = Command.GetSelectedBit(wpfController.command.ram[1, 5], Math.Abs(i - 7));
                tableRA.Rows[1][i] = trisA == 0 ? "o" : "i";

                // Update TRIS values for RB
                int trisB = Command.GetSelectedBit(wpfController.command.ram[1, 6], Math.Abs(i - 7));
                tableRB.Rows[1][i] = trisB == 0 ? "o" : "i";
            }
        }
        private void refreshStack()
        {
            for (int i = 0; i < 8; i++)
            {
                tableStack.Rows[i][0] = wpfController.command.callStack[i];

            }
            CallPos.Text = wpfController.command.callPosition.ToString();
        }  
        private void RunButton(object sender, RoutedEventArgs e)
        {
            
            if (!run)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(0.3);

                // Füge den Event-Handler für das Tick-Ereignis hinzu
                timer.Tick += Run;

                // Starte den Timer
                timer.Start();
                run = true;
                runButton.Background = Brushes.LightGreen; 
            }
            else
            {
                timer.Stop();
                run = false;
                runButton.Background = Brushes.LightGray;
            }
        }

        private void Run(object sender, EventArgs e)
        {
            bool breakpointactive = false; 
            foreach (var breakpoint in LST_File.breakpoints)
            {
                
                int lineIndex = breakpoint.Key;
                if (LST_File.pos == lineIndex)
                {
                    breakpointactive = true;                 
                }
            }
            if (!breakpointactive)
            {               
                OneStep(null, null);
            }
              
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
            if(!Command.sleepModus)
            {
                int command = Fetch();
                if (!Decode(command)) return;
                if(!Command.sleepModus)LST_File.MarkLine(Stack, CodeScroller);
                wpfController.command.EEPROM();
            } 
            Result.Text = "";
            wpfController.command.CheckWriteEEPROM();
            wpfController.command.Mirroring();
            wpfController.InterruptRoutine(Stack);   
            if (Command.sleepModus)
            {
                wpfController.WatchdogRoutine(Stack, 1);
                displayrunTime(1);
            }
            Result.Text = Result.Text + "\n" + "W-Register: " + wpfController.command.wReg + "\n" + "Watchdog: " + Command.watchdog + "\n" + "PCL: " + (Command.PCLATH & 0xFF) + "\n" + "PCLATH: " + (Command.PCLATH & 0x1F00) + "\n" + "SFR: " + (wpfController.command.ram[0,4]);
            refreshUI();
        }

        private void resetLEDs()
        {
            LEDOne.Fill = new SolidColorBrush(Colors.LightGray);
            LEDOTwo.Fill = new SolidColorBrush(Colors.LightGray);
            LEDThree.Fill = new SolidColorBrush(Colors.LightGray);
            LEDFour.Fill = new SolidColorBrush(Colors.LightGray);
            LEDFive.Fill = new SolidColorBrush(Colors.LightGray);
            LEDSix.Fill = new SolidColorBrush(Colors.LightGray);
            LEDSeven.Fill = new SolidColorBrush(Colors.LightGray);
            LEDEight.Fill = new SolidColorBrush(Colors.LightGray);
        }
        private void lightLEDs()
        {
            int port = 6; 
            int intValue = wpfController.command.ram[wpfController.command.bank, port];
            int isOutputValue = wpfController.command.ram[1, port];
            // Create a dictionary mapping indices to LED UI elements
            var leds = new Dictionary<int, System.Windows.Shapes.Ellipse>
            {
                { 0, LEDOne },
                { 1, LEDOTwo },
                { 2, LEDThree },
                { 3, LEDFour },
                { 4, LEDFive },
                { 5, LEDSix },
                { 6, LEDSeven },
                { 7, LEDEight }
            };
            for (int i = 0; i < 8; i++)
            {
                int ledBit = Command.GetSelectedBit(intValue, i);
                int isTrisOutput = Command.GetSelectedBit(isOutputValue, i);
                if (isTrisOutput == 0) 
                {
                    leds[i].Fill = new SolidColorBrush(ledBit == 1 ? Colors.Red : Colors.LightGray);
                }
            }
        }
        private void PrintRaRb()
        {

            for (int i = 7; i >= 0; i--)
            {
                tableRA.Columns.Add("RA" + i.ToString(), typeof(string));
            }
            int storageRA = wpfController.command.ram[wpfController.command.bank, 5];
            DataRow rowRA = tableRA.NewRow();
            int j = 0;
            for (int i = 7; i >= 0; i--)
            {
                rowRA[j] = Command.GetSelectedBit(wpfController.command.ram[wpfController.command.bank, 5], i).ToString();
                j++;
            }
            tableRA.Rows.Add(rowRA);

            DataRow rowTrisRA = tableRA.NewRow();
            j = 0;
            for (int i = 7; i >= 0; i--)
            {
                int value = Command.GetSelectedBit(wpfController.command.ram[1, 5], i);
                if (value == 0)
                {
                    rowTrisRA[j] = "o";
                }
                else
                {
                    rowTrisRA[j] = "i";
                }
                j++;
            }
            tableRA.Rows.Add(rowTrisRA);

            RAGrid.ItemsSource = tableRA.DefaultView;


            // Füge Spalten für RB0 bis RB7 hinzu
            for (int i = 7; i >= 0; i--)
            {
                tableRB.Columns.Add("RB" + i.ToString(), typeof(string));
            }

            DataRow rowRB = tableRB.NewRow();
            int k = 0; 
            for (int i = 7; i >= 0; i--)
            {
                rowRB[k] = Command.GetSelectedBit(wpfController.command.ram[wpfController.command.bank, 6], i).ToString();
                k++; 
            }
            tableRB.Rows.Add(rowRB);

            DataRow rowTrisRB = tableRB.NewRow();
            k = 0;
            for (int i = 7; i >= 0; i--)
            {
                int value = Command.GetSelectedBit(wpfController.command.ram[1, 6], i);
                if(value == 0)
                {
                    rowTrisRB[k] = "o";
                }
                else
                {
                    rowTrisRB[k] = "i";
                }                
                k++;
            }
            tableRB.Rows.Add(rowTrisRB);
            RBGrid.ItemsSource = tableRB.DefaultView;
        }

        private void PrintStack()
        {
            tableStack.Columns.Add("Stack", typeof(int)); 
            
             
            for (int i= 0; i < 8; i++)
            {
                DataRow row = tableStack.NewRow();
                row[0] = wpfController.command.callStack[i];
                tableStack.Rows.Add(row);
            }
            
            StackGrid.ItemsSource = tableStack.DefaultView;
            CallPos.Text = wpfController.command.callPosition.ToString();
        }

        private void PrintSTR()
        {

            tableSTR.Columns.Add("IRP", typeof(int));
            tableSTR.Columns.Add("RP1", typeof(int));
            tableSTR.Columns.Add("RP0" , typeof(int));
            tableSTR.Columns.Add("TO", typeof(int));
            tableSTR.Columns.Add("PD", typeof(int));
            tableSTR.Columns.Add("Z", typeof(int));
            tableSTR.Columns.Add("D", typeof(int));
            tableSTR.Columns.Add("C", typeof(int));


            DataRow row = tableSTR.NewRow();
            int k = 0;
            for (int i = 7; i >= 0; i--)
            {
                row[k] = Command.GetSelectedBit(wpfController.command.ram[wpfController.command.bank, 3], i);
                k++;
            }
            tableSTR.Rows.Add(row);
            STRGrid.ItemsSource = tableSTR.DefaultView;
        }

        private void PrintINTCON()
        {

            tableIntCon.Columns.Add("GIE", typeof(int));
            tableIntCon.Columns.Add("EEIE", typeof(int));
            tableIntCon.Columns.Add("T0IE", typeof(int));
            tableIntCon.Columns.Add("INTE", typeof(int));
            tableIntCon.Columns.Add("RBIE", typeof(int));
            tableIntCon.Columns.Add("T0IF", typeof(int));
            tableIntCon.Columns.Add("INTF", typeof(int));
            tableIntCon.Columns.Add("RBIF", typeof(int));


            DataRow row = tableIntCon.NewRow();
            int k = 0;
            for (int i = 7; i >= 0; i--)
            {
                row[k] = Command.GetSelectedBit(wpfController.command.ram[wpfController.command.bank, 11], i);
                k++;
            }
            tableIntCon.Rows.Add(row);
            INTCONGrid.ItemsSource = tableIntCon.DefaultView;
        }

        private void PrintOption()
        {

            tableOption.Columns.Add("RBPU", typeof(int));
            tableOption.Columns.Add("INTEDG", typeof(int));
            tableOption.Columns.Add("T0CS", typeof(int));
            tableOption.Columns.Add("T0SE", typeof(int));
            tableOption.Columns.Add("PSA", typeof(int));
            tableOption.Columns.Add("PS2", typeof(int));
            tableOption.Columns.Add("PS1", typeof(int));
            tableOption.Columns.Add("PS0", typeof(int));


            DataRow row = tableOption.NewRow();
            int k = 0;
            for (int i = 7; i >= 0; i--)
            {
                row[k] = Command.GetSelectedBit(wpfController.command.ram[1, 1], i);
                k++;
            }
            tableOption.Rows.Add(row);
            OptionGrid.ItemsSource = tableOption.DefaultView;
        }

        private void PrintRam()
        {
            DataTable dt = new DataTable();
            int nbColumns = 8;
            int nbRows = 32;

            for (int i = 0; i < nbColumns; i++)
            {
                dt.Columns.Add(i.ToString(), typeof(string));
            }
            int zaehler = 0;
            int tmpBank = 0;
            for (int row = 0; row < nbRows; row++)
            {
                DataRow dr = dt.NewRow();            
                for (int i = 0; i < nbColumns; i++)
                {
                    dr[i] = wpfController.command.ram[tmpBank, zaehler].ToString("X");
                    zaehler++;

                }
                if (zaehler == 128)
                {
                    zaehler = 0;
                    tmpBank = 1;
                }
                dt.Rows.Add(dr);
                
            }
            MyDataGrid.ItemsSource = dt.DefaultView;
            dt.RowChanged += dtRowChanged;
            

        }

        private void dtRowChanged(object sender, DataRowChangeEventArgs e)
        {
            DataRow changedRow = e.Row;
            DataTable table = changedRow.Table;
            int rowIndex = table.Rows.IndexOf(changedRow);
            String[] intArray = ConvertRowToIntArray(changedRow);
            int i = 0; 
            if(rowIndex > 15)
            {
                i = 1;
                rowIndex = rowIndex - 16; // Das muss gemacht werden da es im dargestellten ram alles in einer Tabelle hängt aber im speicher aufgeteilt wird auf Command.bank 1 und 0
            }
            int rowstart = rowIndex * 8; 

            for(int j = 0; j <8; j++)
            {
                if(Convert.ToInt32(intArray[j], 16) > 255)
                {
                    wpfController.command.ram[i, (rowstart + j)] = 0; 
                }
                else
                {
                    wpfController.command.ram[i, (rowstart + j)] = Convert.ToInt32(intArray[j], 16); 
                }
                  
                Trace.WriteLine(wpfController.command.ram[i, (rowstart + j)]); 
            }
             
        }

        private String[] ConvertRowToIntArray(DataRow row)
        {
            // Neues int-Array erstellen
            String[] intArray = new String[row.ItemArray.Length];

            // Daten aus der DataRow in das int-Array kopieren
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                intArray[i] = Convert.ToString(row[i]);
            }

            return intArray;
        }

        private int Fetch()
        {
            int programCounter = wpfController.command.ram[wpfController.command.bank, 2];
            int command = commands[programCounter];
            programCounter++;
            wpfController.command.ChangePCLATH(wpfController.command.ram[wpfController.command.bank, 2] + 1);
            return command;
        }
        private void displayrunTime(int deltaT)
        {

            runTime += ((deltaT * 4000000.00) / Command.quarzfrequenz);
            Laufzeitzaehler.Text = runTime.ToString();
        }

        private bool Decode(int command)
        {
            int deltaT = 0;
            if ((command & 0x3F00) == 0x3000)
            {
                deltaT = wpfController.command.MOVLW(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0080)
            {
                deltaT = wpfController.command.MOVWF(command & 0x7F);
            }
            if ((command & 0x3F80) == 0x0780 || (command & 0x3F80) == 0x0700)
            {
                deltaT = wpfController.command.ADDWF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0500 || (command & 0x3F80) == 0x0580)
            {
                deltaT = wpfController.command.ANDWF(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3E00)
            {
                deltaT = wpfController.command.ADDLW(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3900)
            {
                deltaT = wpfController.command.ANDLW(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0180)
            {
                deltaT = wpfController.command.CLRF(command & 0x7F);
            }
            if ((command & 0x3F80) == 0x0100)
            {
                deltaT = wpfController.command.CLRW();
            }
            if ((command & 0x3F80) == 0x0980 || (command & 0x3F80) == 0x0900)
            {
                deltaT = wpfController.command.COMF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0380 || (command & 0x3F80) == 0x0300)
            {
                deltaT = wpfController.command.DECF(command & 0xFF);
            }
            if ((command & 0x3800) == 0x2000)
            {
                deltaT = wpfController.CallRoutine(command & 0xFF, Stack);
            }
            if ((command & 0xFFFF) == 0x0008)
            {
                deltaT = wpfController.RETURNRoutine(Stack);
            }
            if ((command & 0x3800) == 0x2800)
            {
                deltaT = wpfController.GoToRoutine(command & 0x7FF, Stack);
            }
            if ((command & 0xFC00) == 0x3400)
            {
                deltaT = wpfController.RetLwRoutine(command & 0xFF, Stack);
            }
            if ((command & 0x3F80) == 0x0B80 || (command & 0x3F80) == 0x0B00)
            {
                deltaT = wpfController.DecFszRoutine(command & 0xFF, Stack);
            }
            if ((command & 0x3F80) == 0x0A80 || (command & 0x3F80) == 0x0A00)
            {
                deltaT = wpfController.command.INCF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0F80 || (command & 0x3F80) == 0xF00)
            {
                deltaT = wpfController.IncFszRoutine(command & 0xFF, Stack);
            }
            if ((command & 0x3F80) == 0x0480 || (command & 0x3F80) == 0x0400)
            {
                deltaT = wpfController.command.IORWF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0880 || (command & 0x3F80) == 0x0800)
            {
                deltaT = wpfController.command.MOVF(command & 0xFF);
            }
            if ((command & 0xFFFF) == 0x0000)
            {
                deltaT = Command.NOP();
            }
            if ((command & 0x3F80) == 0x0D80 || (command & 0x3F80) == 0x0D00)
            {
                deltaT = wpfController.command.RLF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0C80 || (command & 0x3F80) == 0x0C00)
            {
                deltaT = wpfController.command.RRF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0680 || (command & 0x3F80) == 0x0600)
            {
                deltaT = wpfController.command.XORWF(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3A00)
            {
                deltaT = wpfController.command.XORLW(command & 0xFF);
            }
            if ((command & 0x3C00) == 0x1000)
            {
                deltaT = wpfController.command.BCF(command & 0x03FF);
            }
            if ((command & 0x3C00) == 0x1400)
            {
                deltaT = wpfController.command.BSF(command & 0x03FF);
            }
            if ((command & 0x3C00) == 0x1800)
            {
                deltaT = wpfController.BitFscRoutine(command & 0x03FF, Stack);
            }
            if ((command & 0x3C00) == 0x1C00)
            {
                deltaT = wpfController.BitFssRoutine(command & 0x03FF, Stack);
            }
            if ((command & 0x3F00) == 0x0E00)
            {
                deltaT = wpfController.command.SWAPF(command & 0xFF);
            }
            if ((command & 0x3F80) == 0x0280 || (command & 0x3F80) == 0x0200)
            {
                deltaT = wpfController.command.SUBWF(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3800)
            {
                deltaT = wpfController.command.IORLW(command & 0xFF);
            }
            if ((command & 0x3F00) == 0x3C00)
            {
                deltaT = wpfController.command.SUBLW(command & 0xFF);
            }
            if((command & 0xFFFF) == 0x0060)
            {
                deltaT = wpfController.command.CLRWDT();
            }
            if((command & 0xFFFF) == 0x0009)
            {
                deltaT = wpfController.RetFieRoutine(Stack);
            }
            if((command & 0xFFFF) == 0x0063)
            {
                wpfController.command.SLEEP();
            }
            if(!((command & 0x3F80) == 0x0080 && (command & 0x7F) == 1)) wpfController.Timer0Routine(Stack,deltaT);
            wpfController.WatchdogRoutine(Stack,deltaT);
            displayrunTime(deltaT);
            return true;
        }

        private void quarzfrequenz_Four(object sender, RoutedEventArgs e)
        {
            Command.setQuarzfrequenz(4000000);
        }

        private void quarzfrequenz_Eight(object sender, RoutedEventArgs e)
        {
            Command.setQuarzfrequenz(8000000);
        }

        private void quarzfrequenz_Sixteen(object sender, RoutedEventArgs e)
        {
            Command.setQuarzfrequenz(16000000);
        }

        private void quarzfrequenz_Thrittwo(object sender, RoutedEventArgs e)
        {
            Command.setQuarzfrequenz(32000);
        }


        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            wpfController.ResetControllerRoutine(Stack);
            refreshUI();
        }
    }
}
