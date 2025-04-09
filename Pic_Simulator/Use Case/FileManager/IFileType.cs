//https://en.wikipedia.org/wiki/Strategy_pattern
public interface IFileType
{
    public List<string> LoadFile(List<string>list);
    public List<int> GetCommands();
}
