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

        //private Mock<IOutputController> mock;

        [Fact]
        public void Test_LoadFile()
        {
            //Arrange
            LST_File1 test = new LST_File1();
            List<string> lstFile = new List<string>
            {
                "0000 5011",
                "0001 2010",
                "0002 4030",
                "0003 2010"
            };

            List<String> result = test.LoadFile(lstFile);
            Assert.Equal(lstFile, result);
        }
    }
}
