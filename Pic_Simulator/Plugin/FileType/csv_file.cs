

public class CSV_File : IFileType
{
    private List<int> commands = new List<int>();
    public List<int> GetCommands()
    {
        return commands;
    }

    public List<string> LoadFile(List<string> list)
    {
        int counter = 0x0000;
        List<string> formattedOutput = new List<string>();
        foreach (string s in list)
        {
            string firstFour = s.Substring(0, 4);
            if (s.Substring(0, 4) == "    ")
            {
                formattedOutput.Add("        " + s);
                continue;
            }
            else
            {
                firstFour = "0x" + firstFour;
                int value = Convert.ToInt32(firstFour, 16);
                string command = "0x" + s.Substring(5, 4);
                if (value == counter)
                {
                    commands.Add(Convert.ToInt32(command, 16));
                    formattedOutput.Add(s);
                    counter++;
                }
            }
        }
        return formattedOutput;
    }
}

