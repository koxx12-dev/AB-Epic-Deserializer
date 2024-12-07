using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ABEpicBalancingDataContainerDecoder.Helper;
using ABEpicBalancingDataContainerDecoder.Proto;
using ProtoBuf;
using Newtonsoft.Json;
using Sharprompt;

namespace ABEpicBalancingDataContainerDecoder;

internal class Program
{
    public static void Main(string[] args)
    {
        // args = Console.ReadLine()?.Split(' ') ?? args;
        var action =
            GetAction(args.Length < 1 ? Prompt.Select("What do you want to do?", ["decode", "encode"]) : args[0]);

        try
        {
            switch (action)
            {
                case Action.Decode:
                    Decode(args.Length < 1 ? [] : args[1..]);
                    break;
                case Action.Encode:
                    Encode(args.Length < 1 ? [] : args[1..]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void Decode(string[] args)
    {
        var className = args.Length < 1
            ? Prompt.Select("Enter class name", BalancingDataHelper.GetBalancingDataTypes().Select(t => t.Name).ToArray())
            : BalancingDataHelper.ValidateBalancingDataType(args[0]);
        var classpath = BalancingDataHelper.GetBalancingDataPath(className);

        var path = args.Length < 2 ? Prompt.Input<string>("Enter input path in order to read", validators: [ FileExists() ], placeholder: ".\\live_SerializedBalancingDataContainer.bytes") : FileHelper.ValidateFilePath(args[1]);

        var prefix = args.Length < 3 ? Prompt.Input<string>("Enter output path prefix", validators: [ PathExists() ], defaultValue: ".") : args[2];

        var outputPath = Path.Combine(prefix, $"{classpath}.json");

        var balancingDataContainer = ProtoDeserialize<SerializedBalancingDataContainer>(GZipCompressionHelper.DecompressIfNecessary(File.ReadAllBytes(path))).AllBalancingData;

        if (!balancingDataContainer.ContainsKey(classpath))
        {
            throw new Exception("Classname not found in balancing data container");
        }

        var type = BalancingDataHelper.GetBalancingDataTypes().First(t => t.Name == className);
        var arrayType = typeof(List<>).MakeGenericType(type);

        var serializedData = balancingDataContainer[classpath];

        var deserializedData = ProtoDeserialize(serializedData, arrayType);

        File.WriteAllText(outputPath, JsonConvert.SerializeObject(deserializedData, Formatting.Indented));
    }

    private static void Encode(string[] args)
    {
        var className = args.Length < 1
            ? Prompt.Select("Enter class name", BalancingDataHelper.GetBalancingDataTypes().Select(t => t.Name).ToArray())
            : BalancingDataHelper.ValidateBalancingDataType(args[0]);
        var classpath = BalancingDataHelper.GetBalancingDataPath(className);

        var inputPath = args.Length < 2
            ? Prompt.Input<string>("Enter input path in order to read", validators: [FileExists()], placeholder: ".\\live_SerializedBalancingDataContainer.bytes")
            : FileHelper.ValidateFilePath(args[1]);

        var decodedPath = args.Length < 3
            ? Prompt.Input<string>("Enter decoded path in order to read", validators: [FileExists()], placeholder: ".\\ABH.Shared.BalancingData.BALANCING_CLASS.json")
            : FileHelper.ValidateFilePath(args[2]);

        var outputPath = args.Length < 4
            ? Prompt.Input<string>("Enter output path in order to write", defaultValue: inputPath + "_OUT", placeholder: ".\\live_SerializedBalancingDataContainer.bytes_OUT")
            : args[3];

        var compress = args.Length < 5
            ? Prompt.Confirm("Do you compress the file?", defaultValue: true)
            : args[4].ToLower() == "yes" || args[4].ToLower() == "yeah" || args[4].ToLower() == "true" || args[4] == "1" || args[4].ToLower() == "y";

        var balancingDataContainer = ProtoDeserialize<SerializedBalancingDataContainer>(GZipCompressionHelper.DecompressIfNecessary(File.ReadAllBytes(inputPath))).AllBalancingData;

        if (!balancingDataContainer.ContainsKey(classpath))
        {
            throw new Exception("Classname not found in balancing data container");
        }

        var type = BalancingDataHelper.GetBalancingDataTypes().First(t => t.Name == className);
        var arrayType = typeof(List<>).MakeGenericType(type);

        var deserializedPlainJson = JsonDeserialize(File.ReadAllText(decodedPath), arrayType);

        if (deserializedPlainJson == null)
        {
            throw new Exception("Failed to deserialize JSON");
        }

        var serializedData = ProtoSerialize(deserializedPlainJson);

        balancingDataContainer[classpath] = serializedData;

        var reimportedBalancingDataBytes = ProtoSerialize(balancingDataContainer);

        if (compress)
        {
            reimportedBalancingDataBytes = GZipCompressionHelper.Compress(reimportedBalancingDataBytes);
        }

        File.WriteAllBytes(outputPath, reimportedBalancingDataBytes);
    }

    private enum Action
    {
        Decode,
        Encode
    }

    private static Action GetAction(string action)
    {
        return action.ToLower() switch
        {
            "decode" => Action.Decode,
            "encode" => Action.Encode,
            "reimport" => Action.Encode,
            "decrypt" => Action.Decode,
            "encod" => Action.Encode,
            "uncrypt" => Action.Decode,
            _ => throw new ArgumentException("Invalid action")
        };
    }

    public static T ProtoDeserialize<T>(byte[] data) where T : class
    {
        using var stream = new MemoryStream(data);
        return Serializer.Deserialize<T>(stream);
    }

    public static object? ProtoDeserialize(byte[] data, Type type)
    {
        using var stream = new MemoryStream(data);
        return Serializer.Deserialize(type, stream);
    }

    public static byte[] ProtoSerialize(object record)
    {
        using var stream = new MemoryStream();

        Serializer.Serialize(stream, record);
        return stream.ToArray();
    }

    public static T? JsonDeserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static object? JsonDeserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type);
    }

    public static Func<object?, ValidationResult?> FileExists(string? errorMessage = default)
    {
        return input =>
        {
            if (input is not string path)
            {
                return ValidationResult.Success;
            }

            return !File.Exists(path) ? new ValidationResult(errorMessage ?? "Path does not exist") : ValidationResult.Success;
        };
    }

    public static Func<object?, ValidationResult?> PathExists(string? errorMessage = default)
    {
        return input =>
        {
            if (input is not string path)
            {
                return ValidationResult.Success;
            }

            return !Path.Exists(path) ? new ValidationResult(errorMessage ?? "Path is not valid") : ValidationResult.Success;
        };
    }

    public static Func<object?, ValidationResult?> IsBalancingData(string? errorMessage = default)
    {
        return input =>
        {
            if (input is not string className)
            {
                return ValidationResult.Success;
            }

            var balancingDataTypes = BalancingDataHelper.GetBalancingDataTypes();
            return balancingDataTypes.Any(t => t.Name == className) ? ValidationResult.Success : new ValidationResult(errorMessage ?? "Class name does not exist");
        };
    }
}
