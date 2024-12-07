using System.Reflection;

namespace ABEpicBalancingDataContainerDecoder.Helper;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BalancingDataAttribute(string prefix = "ABH.Shared.BalancingData") : Attribute
{
    public string Prefix { get; } = prefix;
}

public static class BalancingDataHelper
{
    private static IEnumerable<Type>? _balancingDataTypes;

    public static IEnumerable<Type> GetBalancingDataTypes()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return _balancingDataTypes ??= assembly.GetTypes().Where(t => t.GetCustomAttribute<BalancingDataAttribute>() != null);
    }

    public static string ValidateBalancingDataType(string typeName)
    {
        var type = GetBalancingDataTypes().FirstOrDefault(t => t.Name == typeName);
        if (type == null)
            throw new ArgumentException($"Balancing data type '{typeName}' not found.");

        return typeName;
    }

    public static string GetBalancingDataPrefix(Type type)
    {
        var attribute = type.GetCustomAttribute<BalancingDataAttribute>();
        if (attribute == null)
            throw new ArgumentException($"Balancing data attribute not found for type '{type.Name}'.");
        return attribute.Prefix;
    }

    public static string GetBalancingDataPrefix(string typeName)
    {
        var type = GetBalancingDataTypes().FirstOrDefault(t => t.Name == typeName);
        if (type == null)
            throw new ArgumentException($"Balancing data type '{typeName}' not found.");
        return GetBalancingDataPrefix(type);
    }

    public static string GetBalancingDataPath(Type type)
    {
        var prefix = GetBalancingDataPrefix(type);
        return prefix + "." + type.Name;
    }

    public static string GetBalancingDataPath(string typeName)
    {
        var type = GetBalancingDataTypes().FirstOrDefault(t => t.Name == typeName);
        if (type == null)
            throw new ArgumentException($"Balancing data type '{typeName}' not found.");
        return GetBalancingDataPath(type);
    }
}
