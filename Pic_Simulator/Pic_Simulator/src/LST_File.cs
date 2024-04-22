
using Pic_Simulator;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

public class LST_File()
{
    static bool loadedFile = false;
    static int fileSize;
    static int startPos;
    public static void LoadFile(StackPanel stack)
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
            MarkLine();
        }
    }

    public static void MarkLine()
    {
        /*if (!loadedFile) return;
        if (pos > fileSize) return;
        if (pos == 0)
        {
            pos = startPos;
            TextBox t = (TextBox)Stack.Children[pos];
            t.Background = Brushes.OrangeRed;
            CodeScroller.ScrollToVerticalOffset(CodeScroller.VerticalOffset + 25 * (startPos - 4));
            return;
        }
        TextBox text = (TextBox)Stack.Children[pos];
        text.Background = Brushes.White;
        pos++;
        text = (TextBox)Stack.Children[pos];
        text.Background = Brushes.OrangeRed;
        CodeScroller.ScrollToVerticalOffset(startPos + 25 * (pos - 4));*/
    }
}


