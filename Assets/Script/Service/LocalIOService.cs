using System.IO;

public class LocalIOService
{
  private string _rootPath;
  public LocalIOService(string root)
  {
    _rootPath = root;
  }
  public void SaveJson(string relativePath, string json)
  {
    string path = Path.Combine(_rootPath, relativePath);

    string directory = Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(directory))
    {
      Directory.CreateDirectory(directory);
    }

    File.WriteAllText(path, json);
  }
  public string ReadJson(string relativePath)
  {
    return File.ReadAllText(Path.Combine(_rootPath, relativePath));
  }
}