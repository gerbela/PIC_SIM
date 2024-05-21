
using Pic_Simulator;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public class LST_File()
{
    public static bool loadedFile = false;
    public static int fileSize;
    static int startPos;
    public static int pos = 0;
    public static  Dictionary<int, TextBlock> breakpoints = new Dictionary<int, TextBlock>();
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
                TextBlock textBox = new TextBlock();
                textBox.Text = file;
                
                textBox.Height = 25;               
                pos++;
                fileSize++;
                textBox.MouseDown += (sender, e) =>
                {
                    TextBox_MouseDoubleClick(sender, e, stack);
                };
                stack.Children.Add(textBox);
            }
            //print commands
            //foreach (int i in commands) Result.Text = Result.Text + i + "\n";
            loadedFile = true;
            Setup(stack, codeScroller);
        }
    }

    private static void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e, StackPanel stackPanel)
    {
        var textBlock = sender as TextBlock;
        if (textBlock != null)
        {
            int lineIndex = stackPanel.Children.IndexOf(textBlock);
            ToggleBreakpoint(lineIndex, textBlock);
        }

    }

    private static void ToggleBreakpoint(int lineIndex, TextBlock textBlock)
    {
        if (breakpoints.ContainsKey(lineIndex))
        {
            breakpoints.Remove(lineIndex);
            textBlock.Background = Brushes.Transparent;
        }
        else
        {
            MessageBox.Show(lineIndex.ToString());
            breakpoints[lineIndex] = textBlock;
            textBlock.Background = Brushes.Red;
        }
    }

    private static void Setup(StackPanel stack, ScrollViewer codeScroller)
    {
        if (!loadedFile) return;
        if (pos == 0)
        {
            pos = startPos;
            TextBlock t = (TextBlock)stack.Children[pos];
            t.Background = Brushes.LightGreen;
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
            t.Background = Brushes.LightGreen;
            codeScroller.ScrollToVerticalOffset(codeScroller.VerticalOffset + 25 * (startPos - 4));
            return;
        }
        TextBlock text = (TextBlock)stack.Children[pos];
        if(breakpoints.Count != 0)
        {
            foreach (var breakpoint in breakpoints)
            {
                int lineIndex = breakpoint.Key;
                TextBlock textBlock = breakpoint.Value;
                // Do something with the line index and TextBlock
                if (lineIndex == pos)
                {
                    text.Background = Brushes.Red;
                    break; 
                    
                }
                else
                {
                    text.Background = Brushes.Transparent;
                }
            }
        }
        else
        {
            text.Background = Brushes.Transparent;
        }
        
        
        pos++;
        TextBlock textnew = (TextBlock)stack.Children[pos];
        

        if (breakpoints.Count != 0)
        {
            foreach (var breakpoint in breakpoints)
            {
                int lineIndex = breakpoint.Key;
                TextBlock textBlock = breakpoint.Value;
                // Do something with the line index and TextBlock
                if (lineIndex == pos)
                {
                    textnew.Background = Brushes.OrangeRed;
                    break; 
                }
                else
                {
                    textnew.Background = Brushes.LightGreen;
                }
            }
        }
        else
        {
            textnew.Background = Brushes.LightGreen;
        }
        
        codeScroller.ScrollToVerticalOffset(startPos + 25 * (pos - 4));
    }

    public static void ClearMarker(StackPanel stack)
    {
        TextBlock text = (TextBlock)stack.Children[pos];

        if (breakpoints.Count != 0)
        {
            foreach (var breakpoint in breakpoints)
            {
                int lineIndex = breakpoint.Key;
                TextBlock textBlock = breakpoint.Value;
                // Do something with the line index and TextBlock
                if (lineIndex == pos)
                {
                    text.Background = Brushes.Red;
                    break; 
                }
                else
                {
                    text.Background = Brushes.Transparent;
                }
            }
        }
        else
        {
            text.Background = Brushes.Transparent;
        }
    }

    public static int FindFilePos(StackPanel stack, int programPos)
    {
        foreach (TextBlock t in stack.Children)
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
        TextBlock t = (TextBlock)stack.Children[pos];
        if (t.Text.StartsWith(" ")) return false;
        int commandPos = Convert.ToInt32(t.Text.Substring(20, 5));
        if (commandPos - 1 == pos) return true;
        return false;
    }

}


