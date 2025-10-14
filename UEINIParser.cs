using System.IO;
using System.Text;

namespace TextureGroupsConfigurator
{
    public class UnrealIniFile
    {
        public Dictionary<string, UnrealIniSection> Sections { get; } = new();
        private readonly List<string> _sectionOrder = new();


        public static UnrealIniFile Load(string path)
        {
            var ini = new UnrealIniFile();
            UnrealIniSection? current = null;

            if (!File.Exists(path))
                return ini;

            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";"))
                    continue;

                if (trimmed.StartsWith("-"))
                    continue;

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    var sectionName = trimmed[1..^1];
                    if (!ini.Sections.TryGetValue(sectionName, out current))
                    {
                        current = new UnrealIniSection(sectionName);
                        ini.Sections[sectionName] = current;
                        ini._sectionOrder.Add(sectionName);
                    }
                    continue;
                }

                if (current is null)
                    continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                current.AddEntry(key, value);
            }

            return ini;
        }

        public void Save(string path)
        {
            if (Sections.Count == 0)
                return;

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var writer = new StreamWriter(path, false, new UTF8Encoding(false));

            foreach (var sectionName in _sectionOrder)
            {
                if (!Sections.TryGetValue(sectionName, out var section))
                    continue;

                writer.WriteLine($"[{section.Name}]");

                foreach (var entry in section.GetAllEntries())
                    writer.WriteLine(entry);

                writer.WriteLine();
            }
        }

        public UnrealIniSection AddSection(string sectionName)
        {
            if (Sections.TryGetValue(sectionName, out var existing))
                return existing;

            var newSection = new UnrealIniSection(sectionName);
            Sections[sectionName] = newSection;
            _sectionOrder.Add(sectionName);

            return newSection;
        }
    }

    public class UnrealIniSection
    {
        public string Name { get; }
        private readonly Dictionary<string, string> _baseEntities = new();
        private readonly List<(string Key, string Value, string Mode)> _entries = new();
        private readonly Dictionary<string, List<string>> _values = new();

        public UnrealIniSection(string name) => Name = name;

        public void RemoveArrayValue(string key, string value)
        {
            if (_values.TryGetValue(key, out var list))
            {
                list.Remove(value);
            }

            _entries.RemoveAll(e => e.Key == key && e.Value == value && e.Mode != "-");
        }

        public void AddEntry(string key, string value)
        {
            string mode = "";
            if (key.StartsWith("+") || key.StartsWith("-") || key.StartsWith("@"))
            {
                mode = key[0].ToString();
                key = key[1..];
            }

            _entries.Add((key, value, mode));

            if (mode == "@")
            {
                _baseEntities[key] = value;
                return;
            }

            if (!_values.ContainsKey(key))
                _values[key] = new List<string>();

            if (mode == "-")
                _values[key].Remove(value);
            else
                _values[key].Add(value);
        }

        public IReadOnlyList<string> GetValues(string key) =>
            _values.TryGetValue(key, out var list) ? list : new List<string>();

        public string? GetValue(string key) =>
            _values.TryGetValue(key, out var list) && list.Count > 0 ? list[^1] : null;

        public IReadOnlyList<string> GetArrayValues(string key) => GetValues(key);

        public bool TryGetBaseEntity(string key, out string value) =>
    _baseEntities.TryGetValue(key, out value);

        public IReadOnlyDictionary<string, string> GetBaseEntities() => _baseEntities;

        public bool TryGetValue(string key, out string value)
        {
            var list = GetValues(key);
            if (list.Count > 0)
            {
                value = list[^1];
                return true;
            }
            value = null!;
            return false;
        }

        public IEnumerable<Dictionary<string, string>> GetStructValues(string key)
        {
            var values = GetValues(key);
            if (values == null) yield break;

            foreach (var v in values)
                yield return UnrealIniValueParser.ParseStruct(v);
        }

        public void SetValue(string key, string value)
        {
            _entries.RemoveAll(e => e.Key == key && e.Mode != "-" && e.Mode != "@");
            _entries.Add((key, value, ""));
            _values[key] = new List<string> { value };
        }

        public void ReplaceArrayValueAt(string key, int index, string newValue)
        {
            if (!_values.TryGetValue(key, out var list)) return;

            if (index < 0 || index >= list.Count) return;

            list[index] = newValue;

            int occurrence = -1;
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Key == key && _entries[i].Mode != "-" && _entries[i].Mode != "@")
                {
                    occurrence++;
                    if (occurrence == index)
                    {
                        _entries[i] = (key, newValue, _entries[i].Mode);
                        return;
                    }
                }
            }

            return;
        }

        public IEnumerable<string> GetAllEntries()
        {
            foreach (var (key, value, mode) in _entries)
            {
                string prefix = mode == "+" || mode == "-" || mode == "@" ? mode : "";
                yield return $"{prefix}{key}={value}";
            }
        }
    }


    public static class UnrealIniValueParser
    {
        public static Dictionary<string, string> ParseStruct(string input)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var trimmed = input.Trim();
            if (trimmed.StartsWith("(") && trimmed.EndsWith(")"))
                trimmed = trimmed[1..^1];

            var parts = SplitRespectingQuotes(trimmed);

            foreach (var part in parts)
            {
                var kv = part.Split('=', 2);
                if (kv.Length != 2) continue;

                var key = kv[0].Trim();
                var value = kv[1].Trim().Trim('"');

                result[key] = value;
            }

            return result;
        }

        private static List<string> SplitRespectingQuotes(string input)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (var ch in input)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(ch);
                }
                else if (ch == ',' && !inQuotes)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            if (current.Length > 0)
                parts.Add(current.ToString());

            return parts;
        }
    }
}