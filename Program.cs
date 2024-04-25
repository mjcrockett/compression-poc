using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;

string cOrD = string.Empty;
while (true)
{
    Console.WriteLine("Compress or Decompress? (c/d)");
    cOrD = Console.ReadLine();
    if (!string.IsNullOrEmpty(cOrD) && (cOrD.ToLower() == "c" || cOrD.ToLower() == "d"))
    {
        break;
    }
}

var helper = new Helper();
if (cOrD == "c")
{
    var memoryBaseline = GC.GetTotalMemory(false) / 1024;

    var uncompressedString = helper.GetUncompressedString();
    byte[] dataToCompress = Encoding.UTF8.GetBytes(uncompressedString);
    var memoryWithUncompressedData = GC.GetTotalMemory(false) / 1024;
    var memorySizeUncompressedData = memoryWithUncompressedData - memoryBaseline;

    byte[] compressedData = helper.Compress(dataToCompress);
    string compressedEncoded = System.Convert.ToBase64String(compressedData); //Save to DB
    var memorySizeCompressedData = helper.GetMemorySizeOfBase64String(compressedEncoded);

    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine(compressedEncoded);
    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("Length of compressed string: " + compressedEncoded.Length);
    Console.WriteLine($"Size in memory of compressed string: {memorySizeCompressedData}");
    Console.WriteLine("Length of uncompressed string: " + uncompressedString.Length);
    Console.WriteLine($"Size in memory of uncompressed string: {memorySizeUncompressedData}KB");
}
else if (cOrD == "d")
{
    var compressedEncoded = helper.GetCompressedBase64String(); //Pull from DB
    var memorySizeCompressedData = helper.GetMemorySizeOfBase64String(compressedEncoded);

    var memoryBaseline = GC.GetTotalMemory(false) / 1024;

    byte[] compressedDataDecoded = System.Convert.FromBase64String(compressedEncoded);
    byte[] decompressedData = helper.Decompress(compressedDataDecoded);
    string deCompressedString = Encoding.UTF8.GetString(decompressedData);
    var memoryWithDecompressedData = GC.GetTotalMemory(false) / 1024;
    var memorySizeDecompressedData = memoryWithDecompressedData - memoryBaseline;

    var niceJsonString = helper.FormatJson(deCompressedString);

    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine(niceJsonString);
    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("Length of decompressed string: " + deCompressedString.Length);
    Console.WriteLine($"Size in memory of decompressed string: {memorySizeDecompressedData}KB");
    Console.WriteLine("Length of compressed string: " + compressedEncoded.Length);
    Console.WriteLine($"Size in memory of compressed string: {memorySizeCompressedData}");
}

Console.ReadLine();

public class Helper
{
    private const int bitsPerBase64Char = 6;
    private const int numOfBitsInAByte = 8;
    public Helper() { }

    public string GetCompressedBase64String()
    {
        var str = File.ReadAllText("compressed.txt");
        return str;
    }

    public string GetUncompressedString()
    {
        var str = File.ReadAllText("uncompressed.txt");
        return str;
    }

    public byte[] Compress(byte[] bytes)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }
            return memoryStream.ToArray();
        }
    }

    public byte[] Decompress(byte[] bytes)
    {
        using (var memoryStream = new MemoryStream(bytes))
        {

            using (var outputStream = new MemoryStream())
            {
                using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    decompressStream.CopyTo(outputStream);
                }
                return outputStream.ToArray();
            }
        }
    }

    public string FormatJson(string json)
    {
        dynamic parsedJson = JsonConvert.DeserializeObject(json);
        return JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
    }

    public string GetMemorySizeOfBase64String(string b64)
    {
        if (b64.Length % 4 != 0)
        {
            throw new Exception("Not a valid base 64 string");
        }
        int bits = b64.Length * bitsPerBase64Char;
        int bytes = bits / numOfBitsInAByte;
        return bytes.ToString() + "Bytes";
    }
}


