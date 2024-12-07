using System.IO.Compression;

namespace ABEpicBalancingDataContainerDecoder.Helper;

public class GZipCompressionHelper
{
    public static byte[] Compress(byte[] data)
    {
        using var memoryStream = new MemoryStream();

        using (Stream stream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            stream.Write(data, 0, data.Length);
        }

        var array = memoryStream.ToArray();
        var result = array ?? throw new ArgumentNullException(nameof(array));

        return result;
    }

    public static byte[] Decompress(byte[] data)
    {
        using var memoryStream = new MemoryStream(data);
        using var stream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var memoryStream2 = new MemoryStream();

        stream.CopyTo(memoryStream2);
        var result = memoryStream2.ToArray();

        return result;
    }

    public static byte[] DecompressIfNecessary(byte[] data)
    {
        byte[] result;
        if (data.Length > 4 && data[0] == 31 && data[1] == 139)
            result = Decompress(data);
        else
            result = data;

        return result;
    }
}
