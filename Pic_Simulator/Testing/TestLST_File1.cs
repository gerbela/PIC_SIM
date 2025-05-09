using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public class TestLST_File1
    {
        private Mock<IFileType> output = new();
        [Fact]
        public void Test_GetCommands()
        {
            var filetype = new Mock<IFileType>();
            List<int> commands = new List<int> { 0, 1, 2, 3 };
            filetype.Setup(x => x.GetCommands()).Returns(commands);
            FileManger manager = new FileManger(filetype.Object);

            List<int> result = manager.GetCommands();

            Assert.Equal(commands, result);
        }
    }
}
