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
        if (value + wReg > 127) // wann wird der gesetzt?
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000010; //Half Carryflag
        }
        if (value + wReg > 256)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000001; // Carryflag
        }
        if ((value + wReg) % 256 == 0)
        {
            ram[bank, 3] = ram[bank, 3] | 0b00000100; // Zeroflag
        }
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
        if (wReg == 0)
        {
            ram[0, 3] = ram[0, 3] | 0b00000100; //Zeroflag
        }
        else
        {
            ram[0, 3] = ram[0, 3] | 0b00000000; //Zeroflag
        }
    }
}