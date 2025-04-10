
public class FileManger
{
    private IFileType fileType;
    public FileManger(IFileType fileType)
    {
        this.fileType = fileType;
    }

    public List<string> LoadFile(List<string> list)
    {
        return fileType.LoadFile(list);
    }

    public List<int> GetCommands()
    {
        return fileType.GetCommands();
    }

}
