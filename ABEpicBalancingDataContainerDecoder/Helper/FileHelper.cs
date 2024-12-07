namespace ABEpicBalancingDataContainerDecoder;

public class FileHelper
{
    public static string ValidateFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            throw new ArgumentException($"File '{filePath}' not found.");

        return filePath;
    }
}
