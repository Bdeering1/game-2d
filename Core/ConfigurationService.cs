using System;
using System.IO;
using Tommy;

namespace Game2D;

public class ConfigurationService
{
    private const string CONFIG_NAME = "dev-config.toml";
    private const string MIN_KEY = "min";
    private const string MAX_KEY = "max";
    private const string CURRENT_KEY = "current";

    private TomlTable table;
    private readonly string configPath;

    public ConfigurationService()
    {
        // will only work for development config
        var parentDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        configPath = Path.Combine(parentDir, CONFIG_NAME);

        Read();
    }

    public void Read()
    {
        using var reader = File.OpenText(configPath);

        try
        {
            table = TOML.Parse(reader);
        }
        catch (TomlParseException e)
        {
            Console.WriteLine($"Unable to read configuration file: {e}");
        }
    }

    public void Write()
    {
        using var writer = File.CreateText(configPath);
        table.WriteTo(writer);
        writer.Flush();
    }

    public int GetInt(string key) => table[key][CURRENT_KEY];
    public int GetInt(string key1, string key2) => table[key1][key2][CURRENT_KEY];
    public void SetInt(string key, int val) => table[key][CURRENT_KEY] = val;
    public void SetInt(string key1, string key2, int val) => table[key1][key2][CURRENT_KEY] = val;

    public float GetFloat(string key) {
        var el = table[key][CURRENT_KEY];
        return el.IsInteger ? (int)el : el;
    }
    public float GetFloat(string key1, string key2) {
        var el = table[key1][key2][CURRENT_KEY];
        return el.IsInteger ? (int)el : el;
    }
    public void SetFloat(string key, float val) => table[key][CURRENT_KEY] = val;
    public void SetFloat(string key1, string key2, float val) => table[key1][key2][CURRENT_KEY] = val;

    public override string ToString() => table.ToString();
}
