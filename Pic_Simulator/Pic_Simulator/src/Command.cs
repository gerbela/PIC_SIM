public class Command
{
    public static int wReg = 0;
    public static int[,] ram = new int[2, 128];
    static int bank = 0;
    public static void ANDWF(int address)
    {
        int result = wReg & ram[bank, address & 0x7F];
        if (result == 0)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000100;
        }
        if ((address & 0x0080) == 0x0080)
        {
            ram[bank, address & 0x007F] = result;
        }
        else
        {
            wReg = result;
        }
    }

    public static void ADDWF(int address)
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

    public static void MovWF(int storageLocation)
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
        if((address & 0x0080) == 0x0080)
        {
            ram[bank, address & 0x7F] = ~ram[bank, address & 0x7F];
        }
        else
        {
            wReg = ~ram[bank, address & 0x7F];
        }
    }

    public static void DECF(int address)
    {
        int result = SUB(ram[bank, address & 0x7F],1);
        if((address & 0x0080) == 0x0080)
        {
            ram[bank, address & 0x7F] = result;
        }
        else
        {
            wReg = result;
        }
    }

    private static int SUB(int valueA, int valueB)
    {
        Zeroflag((valueA - valueB) % 256);
        return (valueA - valueB) % 256; // Wird carry immer aktiv auf 0 gesetzt?
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