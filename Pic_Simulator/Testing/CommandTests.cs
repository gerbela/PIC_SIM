using System.Windows.Input;
using Xunit;
using Moq;
namespace Testing
{
    public class CommandTests
    {
        private Mock<IOutputController> output = new();
        
        [Fact]
        public void Test_ADDLW_normal_Addition()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int flagRegister = command.ram[0, 3]; //Position of the Flagregister
            int arrange = 5;
            command.wReg = 0;

            int result = command.ADDLW(arrange);

            Assert.Equal(arrange, command.wReg); 
            Assert.Equal(0, flagRegister); //For this addition no flag will be set
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDLW_over_max_value()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int flagRegister = command.ram[0, 3]; //Position of the Flagregister
            command.wReg = 255; //max value
            int arrange = 5;

            int result = command.ADDLW(arrange);

            flagRegister = command.ram[0, 3];
            Assert.Equal(3, flagRegister); //Half Carry and Carry bit have to be set
            Assert.Equal(arrange -1, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDLW_test_Zero_flag()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int flagRegister = command.ram[0, 3]; //Position of the Flagregister
            command.wReg = 255; //max value
            int arrange = 1;

            int result = command.ADDLW(arrange);

            flagRegister = command.ram[0, 3];
            Assert.Equal(6, flagRegister); //Third bit of the flag register is set(Zeroflag)
            Assert.Equal(0, command.wReg);
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
        public void Test_MOVWF_Direct_Address()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 10;
            int wReg_Value = 5;
            command.wReg = wReg_Value;

            int result = command.MOVWF(ram_postion);

            Assert.Equal(wReg_Value, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVWF_Indirct_Address()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 0; //Get saving address from ram[0,4]
            int real_pos = 0x000A;
            int wReg_Value = 5;
            command.ram[0, 4] = real_pos;
            command.wReg = wReg_Value;

            int result = command.MOVWF(ram_postion);

            Assert.Equal(wReg_Value, command.ram[0,real_pos]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVWF_Prescaler()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 1;
            int wReg_Value = 5;
            command.wReg = wReg_Value;

            int result = command.MOVWF(ram_postion);

            Assert.Equal(wReg_Value, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_save_in_ram()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            command.ram[0, 10] = 0;
            command.wReg = wReg_Value;

            int result = command.ADDWF(0x008A);

            Assert.Equal(wReg_Value, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_save_in_wReg()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            command.ram[0, ram_postion] = 0;
            command.wReg = wReg_Value;

            int result = command.ADDWF(ram_postion);

            Assert.Equal(0, command.ram[0, ram_postion]);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_ADDWF_Indirct_Address()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int arrange = 0;
            int ram_postion = 0x000A;
            int wReg_Value = 5;
            command.ram[0, ram_postion] = 1;
            command.ram[0, 4] = ram_postion;
            command.wReg = wReg_Value;

            int result = command.ADDWF(arrange);

            Assert.Equal(1, command.ram[0, ram_postion]);
            Assert.Equal(6, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVF_Direct_Address()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int ramPos = 0x000A;
            command.ram[0, ramPos] = 5;

            int result = command.MOVF(ramPos);

            Assert.Equal(5, command.wReg);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Test_MOVF_Indirct_Address()
        {
            var mockOutputController = new Mock<IOutputController>();
            Command command = new Command(mockOutputController.Object);
            int arrange = 0;
            int ramPos = 0x000A;
            command.ram[0, ramPos] = 5;
            command.ram[0, 4] = 0x000A;

            int result = command.MOVF(arrange);

            Assert.Equal(5, command.wReg);
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