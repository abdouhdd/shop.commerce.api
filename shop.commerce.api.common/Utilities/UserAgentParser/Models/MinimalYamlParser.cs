namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Just enough string parsing to recognize the regexes.yaml file format. Introduced to remove
    /// dependency on large Yaml parsing lib. Note that a unit test ensures compatibility
    /// by ensuring regexes and properties are read similar to using the full yaml lib
    /// </summary>
    internal class MinimalYamlParser
    {
        internal class Mapping
        {
            private Dictionary<string, string> _lastEntry;

            public Mapping()
            {
                Sequences = new List<Dictionary<string, string>>();
            }

            public List<Dictionary<string, string>> Sequences { get; }

            public void BeginSequence()
            {
                _lastEntry = new Dictionary<string, string>();
                Sequences.Add(_lastEntry);
            }

            public void AddToSequence(string key, string value)
            {
                _lastEntry[key] = value;
            }
        }

        private readonly Dictionary<string, Mapping> _mappings = new Dictionary<string, Mapping>();

        public MinimalYamlParser(string yamlString)
        {
            ReadIntoMappingModel(yamlString);
        }

        internal IDictionary<string, Mapping> Mappings => _mappings;

        private void ReadIntoMappingModel(string yamlInputString)
        {
            // line splitting using various splitting characters
            string[] lines = yamlInputString.Split(new[] { Environment.NewLine, "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int lineCount = 0;
            Mapping activeMapping = null;

            foreach (var line in lines)
            {
                lineCount++;
                if (line.Trim().StartsWith("#")) //skipping comments
                    continue;
                if (line.Trim().Length == 0)
                    continue;

                //is this a new mapping entity
                if (line[0] != ' ')
                {
                    int indexOfMappingColon = line.IndexOf(':');
                    if (indexOfMappingColon == -1)
                        throw new ArgumentException("YamlParsing: Expecting mapping entry to contain a ':', at line " + lineCount);
                    string name = line.Substring(0, indexOfMappingColon).Trim();
                    activeMapping = new Mapping();
                    _mappings.Add(name, activeMapping);
                    continue;
                }

                //reading scalar entries into the active mapping
                if (activeMapping == null)
                    throw new ArgumentException("YamlParsing: Expecting mapping entry to contain a ':', at line " + lineCount);

                var seqLine = line.Trim();
                if (seqLine[0] == '-')
                {
                    activeMapping.BeginSequence();
                    seqLine = seqLine.Substring(1);
                }

                int indexOfColon = seqLine.IndexOf(':');
                if (indexOfColon == -1)
                    throw new ArgumentException("YamlParsing: Expecting scalar mapping entry to contain a ':', at line " + lineCount);

                string key = seqLine.Substring(0, indexOfColon).Trim();
                string value = ReadQuotedValue(seqLine.Substring(indexOfColon + 1).Trim());
                activeMapping.AddToSequence(key, value);
            }
        }

        private static string ReadQuotedValue(string value)
        {
            if (value.StartsWith("'") && value.EndsWith("'"))
                return value.Substring(1, value.Length - 2);
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);
            return value;
        }

        public IEnumerable<Dictionary<string, string>> ReadMapping(string mappingName)
        {
            if (_mappings.TryGetValue(mappingName, out var mapping))
            {
                foreach (var s in mapping.Sequences)
                {
                    var temp = s;
                    yield return temp;
                }
            }
        }
    }
}
