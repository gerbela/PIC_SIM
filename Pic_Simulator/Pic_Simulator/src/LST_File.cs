
using Pic_Simulator;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


public enum TextColor
{
    Red,
    Transparent,
    OrangeRed,
    LightGreen
}
public class LST_File : IOutputController
{
    public static IFileType filetype = new LST_File1();
    public static FileManger manager = new(filetype);
    public static bool loadedFile = false;
    public static int fileSize;
    static int startPos;
    public static int pos = 0;
    public static  Dictionary<int, TextColor> breakpoints = new Dictionary<int, TextColor>();

    public static bool LoadFile(StackPanel stack, ScrollViewer codeScroller)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.DefaultExt = ".lst";
        dialog.Filter = "Text documents (.lst,.csv)|*.lst;*.csv";
        bool? result = dialog.ShowDialog();
        
        if (result == true)
        {
            if (dialog.FileName.ToLower().EndsWith(".lst"))
            {
                filetype = new LST_File1();
            }
            else if(dialog.FileName.ToLower().EndsWith(".csv"))
            {
                filetype = new CSV_File();
            }
            else
            {
                MessageBox.Show("File type not supported");
                return false;
            }
            stack.Children.Clear();
            breakpoints.Clear();
            MainWindow.commands.Clear();
            List<string> content = File.ReadLines(dialog.FileName).ToList();

            foreach (string s in manager.LoadFile(content))
            {
                TextBlock textBox = new TextBlock();
                textBox.Text = s;

                textBox.Height = 25;
                fileSize++;
                textBox.MouseDown += (sender, e) =>
                {
                    TextBox_MouseDoubleClick(sender, e, stack);
                };
                stack.Children.Add(textBox);
            }
            MainWindow.commands = manager.GetCommands();
            loadedFile = true;
            pos = 0;
            Setup(stack, codeScroller);
            return true;
        }
        return false;
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
            breakpoints[lineIndex] = Change(textBlock);
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

    public static TextColor Change(TextBlock text)
    {
        if(text.Background == Brushes.Red)
        {
            return TextColor.Red;
        }
        else
        {
            return TextColor.Transparent;
        }
    }

    public void JumpToLine(List<string> text, int address, Command command)
    {
        pos = FindFilePos(text, address) - 2;
        command.ram[command.bank, 2] = address;
    }

    public static TextColor SwitchColor()
    {
        foreach (var breakpoint in breakpoints)
        {
            int lineIndex = breakpoint.Key;
            //TextBlock textBlock = breakpoint.Value;
            // Do something with the line index and TextBlock
            if (lineIndex == pos)
            {
                return TextColor.Red;
            }
            else
            {
                return TextColor.Transparent;
            }
        }
        return TextColor.Transparent;
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
                TextColor textBlock = breakpoint.Value;
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
                TextColor textBlock = breakpoint.Value;
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
        if (stack != null)
        {
            TextBlock text = (TextBlock)stack.Children[pos];

            if (breakpoints.Count != 0)
            {
                foreach (var breakpoint in breakpoints)
                {
                    int lineIndex = breakpoint.Key;
                    //TextBlock textBlock = breakpoint.Value;
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
    }

    public static int FindFilePos(List<string> text, int programPos)
    {
        foreach (string s in text)
        {
            if (s.StartsWith(" ")) continue;
            int commandPos = Convert.ToInt32(s.Substring(0, 4), 16);
            if (commandPos == programPos)
            {
                int tmp = Convert.ToInt32(s.Substring(20, 5));
                return tmp;
            }
        }
        return -1;
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

    public int GetPos()
    {
        return pos;
    }

    public Dictionary<int, TextColor> GetBreakpoints()
    {
        return breakpoints;
    }
}


