
public class Command
{
    
    public int wReg = 0;
    public int[,] ram = new int[2, 128];
    public int bank = 0;
    public static int prescaler;
    public static int watchdog;
    public int[] callStack = { 0, 0, 0, 0, 0, 0, 0, 0 };
    public int callPosition = 0;
    private static int setTMR = 0;
    public static int quarzfrequenz = 4000000;
    static int lastEdge = 0;
    static bool prescalerToWatchdog = true;
    static int oldBank = 0;
    static int oldRB0 = 0;
    static int[] oldRBValues = new int[8];
    static int interruptPos = 0;
    public static bool sleepModus = false;
    public static int PCLATH = 0;
    public static int[] EEPROMStorage = new int[64];
    static bool firstWriteEEPROMMuster = false;

    IOutputController outputController;
    public Command(IOutputController outputController)
    {
        this.outputController = outputController;
    }
    public static void setQuarzfrequenz(int newQuarzfrezuenz)
    {
        quarzfrequenz = newQuarzfrezuenz; 
    }

    public int ANDWF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = wReg & ram[bank, address & 0x7F];
        Zeroflag(result);
        DecideSaving(result, address);
        return 1;
    }

    public int ADDWF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = ADD(ram[bank, address & 0x007F], wReg);
        DecideSaving(result, address);
        return 1;
    }
    private int ADD(int value1, int value2)
    {
        HalfCarry(value1, value2);
        Carry(value1 + value2);
        Zeroflag((value1 + value2) % 256);
        return (value1 + value2) & 0xFF;
    }
    public int MOVLW(int literal)
    {
        wReg = literal;
        return 1;
    }

    public int MOVWF(int storageLocation)
    {
        if (storageLocation == 0) storageLocation = ram[bank, 4];
        ram[bank, storageLocation] = wReg;
        if (storageLocation == 1) SetPrescaler();
        return 1;
    }
    public int ADDLW(int literal)
    {
        int result = ADD(literal, wReg);
        wReg = result;
        return 1;
    }

    public int ANDLW(int literal)
    {
        wReg = literal & wReg;
        Zeroflag(wReg);
        return 1;
    }

    public int CLRF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        ram[bank, address] = 0;
        if (bank == 0 && address == 1) SetPrescaler();
        Zeroflag(ram[bank, address]);
        return 1;
    }

    public int CLRW()
    {
        wReg = 0;
        Zeroflag(wReg);
        return 1;
    }
    public int COMF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int value = ram[bank, address & 0x7F];
        int kom = value ^ 0xFF;
        Zeroflag(kom);
        DecideSaving(kom, address);
        return 1;
    }

    public int CALL(int address,List<string> text)
    {
        if (callPosition == 8)
        {
            return -1;
        }
        callStack[callPosition] = ram[bank, 2] - 1;
        ChangePCLATH(address);
        callPosition++;
        outputController.JumpToLine(text, address, this);
        return 2;
    }

    public int DECF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = (ram[bank, address & 0x7F] + 0xFF) % 256;
        Zeroflag(result);//SUB(ram[bank, address & 0x7F],1);
        DecideSaving(result, address);
        return 1;
    }

    public int RETURN(List<string> stack)
    {
        if (callPosition <= 0)
        {
            return -1;
        }
        int address = callStack[callPosition - 1];
        callStack[callPosition - 1] = -1;
        callPosition--;
        outputController.JumpToLine(stack, address + 1, this);
        return 2;
    }
    public int RETFIE(List<string> stack)
    {
        int address = interruptPos;
        outputController.JumpToLine(stack, address + 1, this);
        return 2;
    }

    public int DECFSZ(int address, List<string> stack)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = (ram[bank, address & 0x7F] - 1) % 256;
        DecideSaving(result, address);
        if (result == 0)
        {
            ChangePCLATH(PCLATH + 1);
            outputController.JumpToLine(stack, ram[bank, 2], this);
            return 2;
        }
        return 1;
    }
    public int INCF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = (ram[bank, address & 0x7F] + 1) % 256;
        DecideSaving(result, address);
        Zeroflag(result);
        return 1;
    }
    public int INCFSZ(int address, List<string> stack)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = (ram[bank, address & 0x7F] + 1) % 256;
        DecideSaving(result, address);
        if (result == 0)
        {
            ChangePCLATH(PCLATH + 1);
            outputController.JumpToLine(stack, ram[bank, 2], this);
            return 2;
        }
        return 1;
    }

    public int IORWF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = wReg ^ ram[bank, address & 0x7F];
        DecideSaving(result, address);
        Zeroflag(result);
        return 1;
    }

    public int MOVF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int value = ram[bank, address & 0x7F];
        DecideSaving(value, address);
        Zeroflag(value);
        return 1;
    }

    public static int NOP()
    {
        //Hier wird nichts ausgeführt
        return 1;
    }
    public int RLF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int firstBit = ram[bank, address & 0x7F] & 0x80;
        int carryValueOld = ram[bank, 3] & 0x1;
        if (firstBit == 128)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000001;
        }
        else
        {
            ram[bank, 3] = ram[bank, 3] & 0b11111110;
        }
        int result = (ram[bank, address & 0x7F] << 1) % 256;

        if (carryValueOld == 1)
        {
            result = result + 1;
        }
        DecideSaving(result, address);
        return 1;
    }

    public int RRF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int LasttBit = ram[bank, address & 0x7F] & 0x1;
        int carryValueOld = ram[bank, 3] & 0x1;
        if (LasttBit == 1)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000001;
        }
        else
        {
            ram[bank, 3] = ram[bank, 3] & 0b11111110;
        }
        int result = (ram[bank, address & 0x7F] >> 1) % 256;

        if (carryValueOld == 1)
        {
            result = result + 128;
        }
        DecideSaving(result, address);
        return 1;
    }

    public int XORWF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int result = wReg ^ ram[bank, address & 0x7F];
        DecideSaving(result, address);
        Zeroflag(result);
        return 1;
    }

    public int XORLW(int literal)
    {
        wReg = wReg ^ literal;
        Zeroflag(wReg);
        return 1;
    }

    public int GOTO(int address, List<string> stack)
    {
        ChangePCLATH(address);
        outputController.JumpToLine(stack, ram[bank, 2], this);
        return 2;
    }

    public int RETLW(int value, List<string> stack)
    {
        RETURN(stack);
        wReg = value;
        return 2;
    }

    public int BCF(int address)
    {
        if ((address & 0x7F) == 0) address = (address & 0xFF80) | ram[bank, 4];
        int bit = (address & 0x380) >> 7;
        int rotated = (0x01 << bit) ^ 0xFF;
        int tmp1 = ram[bank, address & 0x7F];
        ram[bank, address & 0x7F] = ram[bank, address & 0x7F] & rotated;
        int tmp = ram[bank, address & 0x7F];
        if ((ram[bank,3] & 0x20) == 0x0) bank = 0;
        return 1;
    }

    public int BSF(int address)
    {
        if ((address & 0x7F) == 0) address = (address & 0xFF80) | ram[bank, 4];
        int bit = (address & 0x380) >> 7;
        int rotated = 0x01 << bit;
        ram[bank, address & 0x7F] = ram[bank, address & 0x7F] | rotated;
        int tmp = ram[bank, 0x3] & 0x20;
        if ((ram[bank, 0x3] & 0x20) == 0x20) bank = 1;
        return 1;
    }
    public int BTFSC(int address, List<string> stack)
    {
        if ((address & 0x7F) == 0) address = (address & 0xFF80) | ram[bank, 4];
        int bit = (address & 0x380) >> 7;
        int rotated = (ram[bank, address & 0x7F] >> bit) & 0x1;
        if (rotated == 1) return 1;
        outputController.JumpToLine(stack, ram[bank, 2] + 1, this);
        return 2;
    }
    public int BTFSS(int address, List<string> stack)
    {
        if ((address & 0x7F) == 0) address = (address & 0xFF80) | ram[bank, 4];
        int bit = (address & 0x380) >> 7;
        int rotated = (ram[bank, address & 0x7F] >> bit) & 0x1;
        if (rotated == 0) return 1;
        outputController.JumpToLine(stack, ram[bank, 2] + 1, this);
        return 2;
    }
    public int SWAPF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int value = ram[bank, address & 0x7F];
        int newUpper = (value & 0x0F) << 4;
        int newLower = (value & 0xF0) >> 4;
        int newValue = newUpper | newLower;
        DecideSaving(newValue, address);
        return 1;
    }
    public int IORLW(int value)
    {
        wReg = wReg | value;
        Zeroflag(wReg);
        return 1;
    }
    public int SUBLW(int value)
    {
        int kom = (wReg ^ 0xFF) + 1;
        int result = ADD(value, kom);
        //kom = (result ^ 0xFF) + 1;
        wReg = result;
        return 1;
    }
    public int SUBWF(int address)
    {
        if ((address & 0x7F) == 0) address = address | ram[bank, 4];
        int kom = (wReg ^ 0xFF) +1;
        int result = ADD(ram[bank, address & 0x7F], kom);
        //kom = (result ^ 0xFF) + 1;
        DecideSaving(result, address);
        return 1;
    }

    public int CLRWDT()
    {
        watchdog = 18000;
        SetPrescaler();
        ram[0, 3] = ram[0, 3] | 0b00011000;
        return 1;
    }
    private void DecideSaving(int value, int address = -1)
    {
        if ((address & 0x0080) == 0x0080)
        {
            if (address == -1) return;
            if (bank == 0 && address == 1) SetPrescaler();
            if (address == 2)
            {
                ChangePCLATH(PCLATH + value);
                return;
            }
            ram[bank, address & 0x7F] = value;
        }
        else
        {
            wReg = value;
        }
    }

    //Methods for setting the falgs in the Status register
    private void Zeroflag(int value)
    {
        if (value == 0)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000100; // Zeroflag
        }
        else
        {
            ram[bank, 3] = ram[bank, 3] & 0b11111011; //Zeroflag
        }
    }

    private void Carry(int value)
    {
        if (value > 256)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000001; // Carryflag
        }
        else
        {
            ram[bank, 3] = ram[bank, 3] & 0b11111110; // Carryflag
        }
    }

    private void HalfCarry(int value1, int value2)
    {
        if (value1 == 256) value1 = 0xF;
        else value1 = value1 & 0xF;
        if (value2 == 256) value2 = 0xF;
        value2 = value2 & 0xF;
        if (value1 + value2 > 15) // wann wird der gesetzt?
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000010; //Half Carryflag
        }
        else
        {
            ram[bank, 3] = ram[bank, 3] & 0b11111101; //Half Carryflag
        }
    }
    public void ChangePCLATH(int value)
    {
        PCLATH = value;
        ram[bank, 2] = PCLATH & 0xFF;
    }

    public static int GetSelectedBit(int value, int pos)
    {
        int bit = 1;
        while (pos != 0)
        {
            bit = bit << 1;
            pos--;
        }
        if ((value & bit) == bit) return 1;
        else return 0;
    }
    

    public static int SetSelectedBit(int value, int pos, int bit)
    {
        int rotatedBit;
        if(bit == 0)
        {
            rotatedBit = 0b01111111;
            pos = 7 - pos;
            rotatedBit = rotatedBit >> pos;
            rotatedBit = (rotatedBit + 1) ^ 0xFF; 
            return value & rotatedBit;
        }
        rotatedBit = 0b00000001;
        rotatedBit = rotatedBit << pos;
        return (value | rotatedBit);
    }

    public void SLEEP()
    {
        ram[bank,3] = SetSelectedBit(ram[bank, 3], 3, 0);
        ram[bank,3] = SetSelectedBit(ram[bank, 3], 4, 1);
        SetPrescaler();
        watchdog = 0;
        sleepModus = true;
    }
    public void WakeUp()
    {
        ram[bank, 3] = SetSelectedBit(ram[bank, 3], 3, 1);
        ram[bank, 3] = SetSelectedBit(ram[bank, 3], 4, 0);
        sleepModus = false;
    }
    public void Timer0(List<string> stack, int steps)
    {
        if (GetSelectedBit(ram[1, 1], 5) == 0)
        {
            if (prescalerToWatchdog)
            {
                ram[0, 1] += steps;
            }
            else
            {
                setTMR += steps;
                if (setTMR >= prescaler)
                {
                    ram[0, 1] += 1;
                    setTMR = setTMR % prescaler;
                }
            }
        }
        else
        {
            //check if option bit 4 is 0 or 1
            if (GetSelectedBit(ram[1, 1], 4) == 0)
            {
                //if option bit 4 is zero reacting on rising edge
                if (GetSelectedBit(ram[0, 5], 4) == 1 && lastEdge == 0)
                {
                    setTMR += 1;
                    if (setTMR >= prescaler)
                    {
                        ram[0, 1] += 1;
                        setTMR = setTMR % prescaler;
                    }
                    lastEdge = 1;
                }
                else if (GetSelectedBit(ram[0, 5], 4) == 0 && lastEdge == 1) lastEdge = 0;
            }
            else
            {
                //if option bit 4 is one reacting on falling edge
                if (GetSelectedBit(ram[0, 5], 4) == 0 && lastEdge == 1)
                {
                    setTMR += 1;
                    if (setTMR >= prescaler)
                    {
                        ram[0, 1] += 1;
                        setTMR = setTMR % prescaler;
                    }
                    lastEdge = 0;
                }
                else if (GetSelectedBit(ram[0, 5], 4) == 1 && lastEdge == 0) lastEdge = 1;
            }
        }
        Timer0Interrupt(stack);
    }

    private void SetPrescaler()
    {
        if (GetSelectedBit(ram[1, 1], 3) == 1)
        {
            prescaler = (int) Math.Pow(2,ram[1, 1] & 0x7);
        }
        else
        {
            int value = (ram[1, 1] & 0x7);
            prescaler = (int)Math.Pow(2, (ram[1, 1] & 0x7)) *2;
        }
        setTMR = 0;
        PSA();
    }
    public void ResetTimer0()
    {
        //Timer
        if (GetSelectedBit(ram[1, 1], 5) == 0)
        {
            ram[0, 1] = 0;
        }
        SetPrescaler();
    }

    public void PSA()
    {
        if (GetSelectedBit(ram[1,1],3) == 0)
        {
            prescalerToWatchdog = false;
        }
        else
        {
            prescalerToWatchdog = true;
        }
    }
    public void Watchdog(List<string> stack, int deltaT)
    {
        deltaT = deltaT * 4000000 / quarzfrequenz;
        if (watchdog + deltaT >= 18000)
        {
            if(prescalerToWatchdog)
            {
                if (prescaler != 0)
                {
                    prescaler--;
                    watchdog = watchdog - 18000;
                }
                else
                {
                    if (sleepModus)
                    {
                        SetSelectedBit(ram[0, 3],4, 0);
                        watchdog = 0;
                        return;
                    }
                    //MessageBox.Show("Some text", "Watchdog", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetController(stack);
                }
            }
            else
            {
                if(sleepModus)
                {
                    SetSelectedBit(ram[0, 3], 4, 0);
                    watchdog = 0;
                    return;
                }
                //MessageBox.Show("Some text", "Watchdog oo", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetController(stack);
            }

        }
        watchdog += deltaT;
    }

    public void Interrupts(List<string> stack)
    {
        RB0Interrupt(stack);
        RB4RB7Interrupt(stack);
    }
    public void Timer0Interrupt(List<string> stack)
    {
        if (ram[0,1] >= 256)
        {
            ram[0, 1] = 0;
            ram[0, 11] = ram[0, 11] | 0b00000100;
            if (GetSelectedBit(ram[0, 11], 2) == 1 && GetSelectedBit(ram[0, 11], 5) == 1 && GetSelectedBit(ram[0, 11], 7) == 1)
            {
                interruptPos = ram[bank, 2] - 1;
                outputController.JumpToLine(stack, 4, this);
                WakeUp();
            }
        }
    }
    public void RB0Interrupt(List<string> stack)
    {
        bool flanke = false;
        if (GetSelectedBit(ram[1, 1], 6) == 1 && oldRB0 == 0 && GetSelectedBit(ram[bank, 6], 0) == 1) flanke = true;
        if (GetSelectedBit(ram[1, 1], 6) == 0 && oldRB0 == 1 && GetSelectedBit(ram[bank, 6], 0) == 0) flanke = true;
        if (oldRB0 == 0 && GetSelectedBit(ram[bank, 6], 0) == 1) oldRB0 = 1;
        if (oldRB0 == 1 && GetSelectedBit(ram[bank, 6], 0) == 0) oldRB0 = 0;
        if (flanke) ram[bank, 11] = SetSelectedBit(ram[bank, 11], 1, 1);
        if (flanke && GetSelectedBit(ram[0, 11], 1) == 1 && GetSelectedBit(ram[0, 11], 4) == 1 && GetSelectedBit(ram[0, 11], 7) == 1)
        {
            interruptPos = ram[bank, 2] - 1;
            outputController.JumpToLine(stack, 4, this);
            WakeUp();
        }
    }

    public void EEPROM()
    {
        if (GetSelectedBit(ram[1,8],0) == 1)
        {
            ReadEEPROMValue();
        }
        if(GetSelectedBit(ram[1, 8], 1) == 1 && GetSelectedBit(ram[1, 8], 2) == 1 && GetSelectedBit(ram[bank, 11], 7) == 0)
        {
            EEPROMStorage[ram[0, 9]] = ram[0, 8];
            ram[1, 8] = SetSelectedBit(ram[1, 8], 4, 1);
        }
    }

    public void ReadEEPROMValue()
    {
        ram[0, 8] = EEPROMStorage[ram[0, 9]];
        ram[1, 8] = SetSelectedBit(ram[1, 8], 0, 0);
    }
    public void WriteEEPROMValue()
    {
        EEPROMStorage[ram[0, 9]] = ram[0, 8];
        ram[1, 8] = SetSelectedBit(ram[1, 8], 4, 1);
    }
    public void CheckWriteEEPROM()
    {
        if (ram[1, 9] == 0x55) firstWriteEEPROMMuster = true;
        if (GetSelectedBit(ram[1, 8], 1) == 1 && GetSelectedBit(ram[1, 8], 2) == 1 && GetSelectedBit(ram[bank, 11], 7) == 0)
        {
            if (firstWriteEEPROMMuster && ram[1, 9] == 0xAA)
            {
                WriteEEPROMValue();
                firstWriteEEPROMMuster = false;
                ram[1,8] = SetSelectedBit(ram[1, 8], 1, 0);
                ram[1,8] = SetSelectedBit(ram[1, 8], 4, 1);
            }
        }
    }
    public void RB4RB7Interrupt(List<string> stack)
    {
        bool isInterrupt = false;
        for (int i = 0; i < 8; i++)
        {
            if (i < 4) continue;
            if (oldRBValues[i] == GetSelectedBit(ram[0, 6], i)) continue;
            if (GetSelectedBit(ram[1, 6], i) == 1)
            {
                isInterrupt = true;
                oldRBValues[i] = GetSelectedBit(ram[0, 6], i);
            }
        }
        if(isInterrupt) ram[bank, 11] = SetSelectedBit(ram[bank, 11], 0, 1);
        if (isInterrupt && GetSelectedBit(ram[0, 11], 0) == 1 && GetSelectedBit(ram[0, 11], 3) == 1 && GetSelectedBit(ram[0, 11], 7) == 1)
        {
            interruptPos = ram[bank, 2] - 1;
            outputController.JumpToLine(stack, 4, this);
            WakeUp();
        }
    }
    public void ResetController(List<string> stack)
    {
        //todo change to reset 0b1111111;
        //ram[1, 1] = 0b11111111;
        ram[1, 1] = 0b11111111;
        ram[0,11] = 0b00100000;
        ram[0, 3] = 0b00011000;
        ram[1, 5] = 0b11111111;
        ram[1, 6] = 0b11111111;
        SetPrescaler();
        if(stack.Count != 0) outputController.JumpToLine(stack, 0, this);
    }

    public void Mirroring()
    {
        if((oldBank == 0 && bank == 0) || (oldBank == 0 && bank == 1))
        {
            ram[1, 2] = ram[0, 2];
            ram[1, 3] = ram[0, 3];
            ram[1, 4] = ram[0, 4];
            ram[1, 10] = ram[0, 10];
            ram[1, 11] = ram[0, 11];
        }
        else
        {
            ram[0, 2] = ram[1, 2];
            ram[0, 3] = ram[1, 3];
            ram[0, 4] = ram[1, 4];
            ram[0, 10] = ram[1, 10];
            ram[0, 11] = ram[1, 11];
        }

        if (oldBank == 0 && bank == 1) oldBank = 1;
        if (oldBank == 1 && bank == 0) oldBank = 0;

    }
    //Set Values in ram that are not 0 at the beginning
    public void startUpRam()
    {
        ram[1, 5] = 0b11111111;
        ram[1, 6] = 0b11111111;
    }
}