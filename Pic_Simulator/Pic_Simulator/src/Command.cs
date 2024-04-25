using System.DirectoryServices;
using System.Net;
using System.Windows.Controls;

public class Command
{
    public static int wReg = 0;
    public static int[,] ram = new int[2, 128];
    public static int bank = 0;
    public static int[] callStack = { -1,-1,-1,-1,-1,-1,-1,-1};
    static int callPosition = 0;

    public static void ANDWF(int address)
    {
        int result = wReg & ram[bank, address & 0x7F];
        Zeroflag(result);
        DecideSaving(result, address);
    }

    public static void ADDWF(int address)
    {
        int result = ADD(ram[bank, address & 0x007F]);
        DecideSaving(result, address);
    }
    private static int ADD(int value)
    {
        HalfCarry(value + wReg);
        Carry(value + wReg);
        Zeroflag((value + wReg) % 256);
        return (value + wReg) % 256; // Wird carry immer aktiv auf 0 gesetzt?
    }
    public static void MOVLW(int literal)
    {
        wReg = literal;
    }

    public static void MOVWF(int storageLocation)
    {
        ram[bank, storageLocation] = wReg;
    }
    public static void ADDLW(int literal)
    {
        int result = ADD(literal);
        wReg = result;
    }

    public static void ANDLW(int literal)
    {
        wReg = literal & wReg;
        Zeroflag(wReg);
    }

    public static void CLRF(int address)
    {
        ram[bank, address] = 0;
    }

    public static void CLRW()
    {
        wReg = 0;
    }
    public static void COMF(int address)
    {
        int value = ram[bank, address & 0x7F];
        int kom = value ^ 0xFF;
        DecideSaving(kom, address);
    }

    public static void CALL(int address, StackPanel stack)
    {
        if(callPosition == 8)
        {
            callPosition = 0;
        }
        callStack[callPosition] = ram[bank, 2] -1;
        ram[bank, 2] = address;
        callPosition++;
        LST_File.JumpToLine(stack, address);
    }

    public static void DECF(int address)
    {
        int result = SUB(ram[bank, address & 0x7F],1);
        DecideSaving(result, address);
    }

    public static bool RETURN(StackPanel stack)
    {
        if (callPosition <= 0)
        {
            LST_File.pos++;
            return false;
        }
        int address = callStack[callPosition - 1];
        callStack[callPosition - 1] = -1;
        callPosition--;
        LST_File.JumpToLine(stack, address +1);
        return true;
    }

    public static void DECFSZ(int address, StackPanel stack )
    {
        int result = (ram[bank, address & 0x7F] - 1) % 256;
        DecideSaving(result, address);
        if(result == 0)
        {
            ram[bank, 2]  +=1;
            LST_File.JumpToLine(stack, ram[bank, 2]);
        }

    }
    public static void INCF(int address)
    {
        int result = (ram[bank, address & 0x7F] + 1) % 256;
        DecideSaving(result, address);
        Zeroflag(result);
    }
    public static void INCFSZ(int address, StackPanel stack)
    {
        int result = (ram[bank, address & 0x7F] + 1) % 256;
        DecideSaving(result, address); wReg = result;
        if (result == 0)
        {
            ram[bank, 2] += 1;
            LST_File.JumpToLine(stack, ram[bank, 2]);
        }
    }

    public static void IORWF(int address)
    {
        int result =  wReg ^ ram[bank, address & 0x7F];
        DecideSaving(result, address);
        Zeroflag(result);
    }

    public static void MOVF(int address)
    {
        int value = ram[bank, address & 0x7F];
        DecideSaving(value, address); 
        Zeroflag(value);
    }

    public static void NOP()
    {
        //Hier wird nichts ausgeführt
    }
    public static void RLF(int address)
    {
        int firstBit = ram[bank, address & 0x7F] & 0x80;
        int carryValueOld = ram[bank, 3] & 0x1;
        if(firstBit == 128)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000001; 
        }
        else
        {
            ram[bank, 3] = ram[bank, 3] & 0b11111110;
        }
        int result = (ram[bank, address & 0x7F] << 1) % 256; 

        if(carryValueOld == 1)
        {
             result = result + 1; 
        }
        DecideSaving(result, address); 
    }

    public static void RRF(int address)
    {
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
    }

    public static void XORWF(int address)
    {
        int result = wReg ^ ram[bank, address & 0x7F];
        DecideSaving(result, address);
        Zeroflag(result);
    }

    public static void XORLW(int literal)
    {
        wReg = wReg ^ literal;
        Zeroflag(wReg);
    }


    private static int SUB(int valueA, int valueB)
    {
        Zeroflag((valueA - valueB) % 256);
        return (valueA - valueB) % 256; // Wird carry immer aktiv auf 0 gesetzt?
    }

    public static void GOTO(int address, StackPanel stack)
    {
        ram[bank, 2] = address;
        LST_File.JumpToLine(stack, address);
    }

    public static bool RETLW(int value, StackPanel stack)
    {
        bool result = RETURN(stack);
        if(result) wReg = value;
        return result;
    }

    public static void BCF(int address)
    {
        int bit = (address & 0x380) >> 7;
        int rotated = (0x01 << bit-1) ^0xFF;
        ram[bank, address & 0x7F] = ram[bank, address & 0x7F] & rotated;
    }

    public static void BSF(int address)
    {
        int bit = (address & 0x380) >> 7;
        int rotated = 0x01 << bit - 1;
        ram[bank, address & 0x7F] = ram[bank, address & 0x7F] | rotated;
    }
    public static void BTFSC(int address,StackPanel stack)
    {
        int bit = (address & 0x380) >> 7;
        int rotated = (ram[bank, address & 0x7F] >> bit - 1) & 0x1;
        if (rotated == 1) return;
        LST_File.JumpToLine(stack, ram[bank, 2] + 1);
    }
    public static void BTFSS(int address, StackPanel stack)
    {
        int bit = (address & 0x380) >> 7;
        int rotated = (ram[bank, address & 0x7F] >> bit - 1) & 0x1;
        if (rotated == 0) return;
        LST_File.JumpToLine(stack, ram[bank, 2] + 1);
    }
    public static void SWAPF(int address)
    {
        int value = ram[bank, address & 0x7F];
        int newUpper = (value & 0x0F) << 4;
        int newLower = (value & 0xF0) >> 4;
        int newValue = newUpper | newLower;
        DecideSaving(newValue, address);
    }
    public static void IORLW(int value)
    {
        wReg = wReg | value;
        Zeroflag(wReg);
    }
    public static void SUBLW(int value)
    {
        int kom = (value ^ 0xFF) + 1;
        int result = ADD(kom);
        wReg = result;
    }
    public static void SUBWF(int address)
    {
        int kom = (ram[bank, address & 0x7F] ^ 0xFF) + 1;
        int result = ADD(kom);
        kom = (result ^ 0xFF) + 1;
        DecideSaving(kom, address);
    }
    private static void DecideSaving(int value, int address = -1)
    {
        if ((address & 0x0080) == 0x0080)
        {
            if (address == -1) return;
            ram[bank, address & 0x7F] = value;
        }
        else
        {
            wReg = value;
        }
    }

    //Methods for setting the falgs in the Status register
    private static void Zeroflag(int value)
    {
        if (value == 0)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000100; // Zeroflag
        }
        else
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000000; //Zeroflag
        }
    }

    private static void Carry(int value)
    {
        if (value > 256)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000001; // Carryflag
        }
    }

    private static void HalfCarry(int value)
    {
        if (value > 127) // wann wird der gesetzt?
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000010; //Half Carryflag
        }
    }
}