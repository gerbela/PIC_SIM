
public class FileManager
{
    private IFileType fileType;
    public FileManager(IFileType fileType)
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

    public void ChangeFileType(IFileType fileType)
    {
        this.fileType = fileType;
    }

}
