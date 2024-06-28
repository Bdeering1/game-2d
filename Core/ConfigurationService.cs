using System;
using System.IO;
using System.Collections.Generic;
using Tomlyn;

namespace Game2D;

using Table = Dictionary<string, Dictionary<string, List<object>>>;

public class ConfigurationService
{
    private const string CONFIG_NAME = "dev-config.toml";
    private const int CURRENT_IDX = 0;
    private const int MIN_IDX = 1;
    private const int MAX_IDX = 2;

    private Table table;
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
        table = Toml.ToModel<Table>(File.ReadAllText(configPath));
    }

    public void Write()
    {
        File.WriteAllText(configPath, Toml.FromModel(table));
    }

    public object GetValue(string key1, string key2) {
        var el = table[key1][key2][CURRENT_IDX];
        return el switch {
            double d => Convert.ToSingle(d),
            long l => Convert.ToInt32(l),
            _ => el
        };
    }

    public void SetValue(string key1, string key2, object value) =>
        table[key1][key2][CURRENT_IDX] = value;
}
