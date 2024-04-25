
using Pic_Simulator;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

public class LST_File()
{
    public static bool loadedFile = false;
    public static int fileSize;
    static int startPos;
    public static int pos = 0;
    public static void LoadFile(StackPanel stack, ScrollViewer codeScroller)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.DefaultExt = ".lst";
        dialog.Filter = "Text documents (.lst)|*.lst";
        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            int counter = 0x0000;
            int pos = 1;

            foreach (string s in File.ReadLines(dialog.FileName))
            {
                string file = "";

                string firstFour = s.Substring(0, 4);
                if (s.Substring(0, 4) == "    ")
                {
                    string tmp = "        " + s;
                    file = file + tmp;
                }
                else
                {
                    firstFour = "0x" + firstFour;
                    int value = Convert.ToInt32(firstFour, 16);
                    string command = "0x" + s.Substring(5, 4);
                    if (value == counter)
                    {
                        if (value == 0) startPos = pos - 1;
                        MainWindow.commands.Add(Convert.ToInt32(command, 16));
                        file = file + s;
                        counter++;
                    }
                }
                TextBox textBox = new TextBox();
                textBox.Text = file;
                textBox.IsReadOnly = true;
                textBox.Height = 25;
                stack.Children.Add(textBox);
                pos++;
                fileSize++;

            }
            //print commands
            //foreach (int i in commands) Result.Text = Result.Text + i + "\n";
            loadedFile = true;
            Setup(stack, codeScroller);
        }
    }

    private static void Setup(StackPanel stack, ScrollViewer codeScroller)
    {
        if (!loadedFile) return;
        if (pos == 0)
        {
            pos = startPos;
            TextBox t = (TextBox)stack.Children[pos];
            t.Background = Brushes.OrangeRed;
            codeScroller.ScrollToVerticalOffset(codeScroller.VerticalOffset + 25 * (startPos - 4));
            return;
        }
    }

    public static void JumpToLine(StackPanel stack, int address)
    {
        ClearMarker(stack);
        pos = FindFilePos(stack, address) - 2;
        Command.ram[Command.bank, 2] = address;
    }
    public static void MarkLine(StackPanel stack, ScrollViewer codeScroller)
    {
        if (!loadedFile) return;
        if (pos > fileSize) return;
        if (pos == 0)
        {
            pos = startPos;
            TextBox t = (TextBox)stack.Children[pos];
            t.Background = Brushes.OrangeRed;
            codeScroller.ScrollToVerticalOffset(codeScroller.VerticalOffset + 25 * (startPos - 4));
            return;
        }
        TextBox text = (TextBox)stack.Children[pos];
        text.Background = Brushes.White;
        pos++;
        text = (TextBox)stack.Children[pos];
        text.Background = Brushes.OrangeRed;
        codeScroller.ScrollToVerticalOffset(startPos + 25 * (pos - 4));
    }

    public static void ClearMarker(StackPanel stack)
    {
        TextBox text = (TextBox)stack.Children[pos];
        text.Background = Brushes.White;
    }

    public static int FindFilePos(StackPanel stack, int programPos)
    {
        foreach (TextBox t in stack.Children)
        {
            if (t.Text.StartsWith(" ")) continue;
            int commandPos = Convert.ToInt32(t.Text.Substring(0, 4), 16);
            if (commandPos == programPos)
            {
                int tmp = Convert.ToInt32(t.Text.Substring(20, 5));
                return tmp;
            }
        }
        return -1;
    }

    public static Boolean CheckCommand(StackPanel stack)
    {
        TextBox t = (TextBox)stack.Children[pos];
        if (t.Text.StartsWith(" ")) return false;
        int commandPos = Convert.ToInt32(t.Text.Substring(20, 5));
        if (commandPos - 1 == pos) return true;
        return false;
    }

}


