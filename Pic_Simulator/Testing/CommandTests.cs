using System.Windows.Input;
using Xunit;
namespace Testing
{
    public class CommandTests
    {
        [Fact]
        public void Test_ADDLW_empty_wReg()
        {
            int arrange = 5;
            Command.wReg = 0;

            int result = Command.ADDLW(arrange);

            Assert.Equal(arrange, Command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDLW_non_empty_wReg()
        {
            Command.wReg = 5;
            int arrange = 5;

            int result = Command.ADDLW(arrange);

            Assert.Equal(10, Command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVLW()
        {
            int arrange = 5;

            int result = Command.MOVLW(arrange);

            Assert.Equal(arrange, Command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVWF()
        {
            int ram_postion = 10;
            int wReg_Value = 5;
            Command.bank = 0;
            Command.wReg = wReg_Value;

            int result = Command.MOVWF(ram_postion);

            Assert.Equal(wReg_Value, Command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_empty_ram_save_in_ram()
        {
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            Command.ram[0, 10] = 0;
            Command.bank = 0;
            Command.wReg = wReg_Value;

            int result = Command.ADDWF(0x008A);

            Assert.Equal(wReg_Value, Command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_non_empty_ram_save_in_ram()
        {
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            Command.ram[0, ram_postion] = 5;
            Command.bank = 0;
            Command.wReg = wReg_Value;

            int result = Command.ADDWF(0x008A);

            Assert.Equal(10, Command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_empty_ram_save_in_wReg()
        {
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            Command.ram[0, ram_postion] = 0;
            Command.bank = 0;
            Command.wReg = wReg_Value;

            int result = Command.ADDWF(ram_postion);

            Assert.Equal(0, Command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_non_empty_ram_save_in_wReg()
        {
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            Command.ram[0, ram_postion] = 5;
            Command.bank = 0;
            Command.wReg = wReg_Value;

            int result = Command.ADDWF(ram_postion);

            Assert.Equal(5, Command.ram[0, ram_postion]);
            Assert.Equal(10, Command.wReg);
            Assert.Equal(1, result);
        }
    }
}