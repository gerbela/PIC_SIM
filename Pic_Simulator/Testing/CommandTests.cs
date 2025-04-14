using System.Windows.Input;
using Xunit;
using Moq;
namespace Testing
{
    public class CommandTests
    {
        private Mock<IOutputController> output = new();
        [Fact]
        public void Test_ADDLW_empty_wReg()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int arrange = 5;
            command.wReg = 0;

            int result = command.ADDLW(arrange);

            Assert.Equal(arrange, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDLW_non_empty_wReg()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            command.wReg = 5;
            int arrange = 5;

            int result = command.ADDLW(arrange);

            Assert.Equal(10, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDLW_negative_input()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            command.wReg = 5;
            int arrange = -5;

            int result = command.ADDLW(arrange);

            Assert.Equal(0, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDLW_negative_result()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            command.wReg = 5;
            int arrange = -6;

            int result = command.ADDLW(arrange);

            Assert.Equal(255, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVLW()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int arrange = 5;

            int result = command.MOVLW(arrange);

            Assert.Equal(arrange, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVWF()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 10;
            int wReg_Value = 5;
            command.bank = 0;
            command.wReg = wReg_Value;

            int result = command.MOVWF(ram_postion);

            Assert.Equal(wReg_Value, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_empty_ram_save_in_ram()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            command.ram[0, 10] = 0;
            command.bank = 0;
            command.wReg = wReg_Value;

            int result = command.ADDWF(0x008A);

            Assert.Equal(wReg_Value, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_non_empty_ram_save_in_ram()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            command.ram[0, ram_postion] = 5;
            command.bank = 0;
            command.wReg = wReg_Value;

            int result = command.ADDWF(0x008A);

            Assert.Equal(10, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_empty_ram_save_in_wReg()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            command.ram[0, ram_postion] = 0;
            command.bank = 0;
            command.wReg = wReg_Value;

            int result = command.ADDWF(ram_postion);

            Assert.Equal(0, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_non_empty_ram_save_in_wReg()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            command.ram[0, ram_postion] = 5;
            command.bank = 0;
            command.wReg = wReg_Value;

            int result = command.ADDWF(ram_postion);

            Assert.Equal(5, command.ram[0, ram_postion]);
            Assert.Equal(10, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_RETURN()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            command.callStack[command.callPosition] = 1;
            command.callPosition++;

            int result = command.RETURN(new List<string>());

            Assert.Equal(0, command.callPosition);
            Assert.Equal(2, result);
            Assert.Equal(-1, command.callStack[command.callPosition]);
        }
    }
}