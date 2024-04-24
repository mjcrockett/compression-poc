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
    var memoryWithCompressedData = GC.GetTotalMemory(false) / 1024;
    var memorySizeCompressedData = memoryWithCompressedData - memoryWithUncompressedData;

    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine(compressedEncoded);
    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("Length of compressed string: " + compressedEncoded.Length);
    Console.WriteLine($"Size in memory of compressed string: {memorySizeCompressedData}KB");
    Console.WriteLine("Length of uncompressed string: " + uncompressedString.Length);
    Console.WriteLine($"Size in memory of uncompressed string: {memorySizeUncompressedData}KB");
}
else if (cOrD == "d")
{
    var memoryBaseline = GC.GetTotalMemory(false) / 1024;

    var compressedString = helper.GetCompressedString(); //Pull from DB
    var memoryWithCompressedData = GC.GetTotalMemory(false) / 1024;
    var memorySizeCompressedData = memoryWithCompressedData - memoryBaseline;

    byte[] compressedDataDecoded = System.Convert.FromBase64String(compressedString);
    byte[] decompressedData = helper.Decompress(compressedDataDecoded);
    string deCompressedString = Encoding.UTF8.GetString(decompressedData);
    var memoryWithDecompressedData = GC.GetTotalMemory(false) / 1024;
    var memorySizeDecompressedData = memoryWithDecompressedData - memoryWithCompressedData;

    var niceJsonString = helper.FormatJson(deCompressedString);

    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine(niceJsonString);
    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("Length of decompressed string: " + deCompressedString.Length);
    Console.WriteLine($"Size in memory of decompressed string: {memorySizeDecompressedData}KB");
    Console.WriteLine("Length of compressed string: " + compressedString.Length);
    Console.WriteLine($"Size in memory of compressed string: {memorySizeCompressedData}KB");
}

Console.ReadLine();

public class Helper
{
    public Helper() { }

    public string GetCompressedString()
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
}


