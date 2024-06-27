using System;
using System.IO;
using Tommy;

namespace Game2D;

public class ConfigurationService
{
    private const string CONFIG_NAME = "dev-config.toml";

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

    public float GetInt(string key) => table[key].AsInteger;
    public float GetInt(string key1, string key2) => table[key1][key2].AsInteger;
    public void SetInt(string key, int val) => table[key] = val;
    public void SetInt(string key1, string key2, int val) => table[key1][key2] = val;

    public float GetFloat(string key) => table[key].AsFloat;
    public float GetFloat(string key1, string key2) => table[key1][key2].AsFloat;
    public void SetFloat(string key, float val) => table[key] = val;
    public void SetFloat(string key1, string key2, float val) => table[key1][key2] = val;
}
