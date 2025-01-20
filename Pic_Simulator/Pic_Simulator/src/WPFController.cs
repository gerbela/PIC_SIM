using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

public class WPFController
{
    public Command command;
    public LST_File lst_file;
    public IOutputController outputController;

    public WPFController(IOutputController outputController)
    {
        this.outputController = outputController;
        command = new Command(outputController);
        lst_file = new LST_File();
    }

    public int CallRoutine(int address, StackPanel stack)
    {
        ClearMarker(stack);
        List<string> panelLines = StackpanelToList(stack);
        int steps = command.CALL(address, panelLines);
        //outputController.JumpToLine(address);
        return steps;
    }

    public int GoToRoutine(int address, StackPanel stack)
    {
        ClearMarker(stack);
        List<string> panelLines = StackpanelToList(stack);
        int steps = command.GOTO(address, panelLines);
        return steps;
    }

    public int RETURNRoutine(StackPanel stack)
    {
        ClearMarker(stack);
        List<string> panelLines = StackpanelToList(stack);
        return command.RETURN(panelLines);
    }

    public int RetLwRoutine(int address, StackPanel stack)
    {
        ClearMarker(stack);
        List<string> panelLines = StackpanelToList(stack);
        return command.RETLW(address, panelLines);
    }

    public int DecFszRoutine(int address, StackPanel stack)
    {
        List<string> panelLines = StackpanelToList(stack);
        return command.DECFSZ(address, panelLines);
    }

    public int IncFszRoutine(int address, StackPanel stack)
    {
        List<string> panelLines = StackpanelToList(stack);
        return command.INCFSZ(address, panelLines);
    }

    public int BitFscRoutine(int address, StackPanel stack)
    {
        List<string> panelLines = StackpanelToList(stack);
        return command.BTFSC(address, panelLines);
    }

    public int BitFssRoutine(int address, StackPanel stack)
    {
        List<string> panelLines = StackpanelToList(stack);
        return command.BTFSS(address, panelLines);
    }

    public int RetFieRoutine(StackPanel stack)
    {
        List<string> panelLines = StackpanelToList(stack);
        return command.RETFIE(panelLines);
    }

    public void WatchdogRoutine(StackPanel stack, int steps)
    {
        List<string> panelLines = StackpanelToList(stack);
        command.Watchdog(panelLines, steps);
    }

    public void ResetControllerRoutine(StackPanel stack)
    {
        ClearMarker(stack);
        List<string> panelLines = StackpanelToList(stack);
        command.ResetController(panelLines);
    }

    public void Timer0Routine(StackPanel stack, int steps)
    {
        List<string> panelLines = StackpanelToList(stack);
        command.Timer0(panelLines, steps);
    }

    public void InterruptRoutine(StackPanel stack)
    {
        List<string> panelLines = StackpanelToList(stack);
        command.Interrupts(panelLines);
    }

    public void JumpToLine(int address, StackPanel stack)
    {
        ClearMarker(stack);
        List<string> panelLines = StackpanelToList(stack);
        outputController.JumpToLine(panelLines, address);
    }

    public List<string> StackpanelToList(StackPanel stack)
    {
        List<string> text = new List<string>();
        foreach (TextBlock t in stack.Children)
        {
            text.Add(t.Text);
        }
        return text;
    }
    public void ClearMarker(StackPanel stack)
    {
        if (stack != null)
        {
            int pos = lst_file.GetPos();
            Dictionary<int,TextColor> breakpoints = lst_file.GetBreakpoints();
            TextBlock text = (TextBlock)stack.Children[pos];

            if (breakpoints.Count != 0)
            {
                TextColor color = LST_File.SwitchColor();
                text.Background = GetTextColor(color);
                
            }
            else
            {
                text.Background = Brushes.Transparent;
            }
        }
    }

    public Brush GetTextColor(TextColor textColor)
    {
        if(TextColor.Red == textColor)
        {
            return Brushes.Red;
        }
        else
        {
            return Brushes.Transparent;
        }
    }

}
