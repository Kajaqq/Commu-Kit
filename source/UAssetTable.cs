using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace Commu_Kit
{
    internal class UAssetTable
    {
        private const EngineVersion UNREAL_VERSION = EngineVersion.VER_UE4_24;
        private readonly UAsset openFile;
        private readonly UDataTable table;

        private MessageList messages = null;

        public UAssetTable(string uAssetPath)
        {
            openFile = new UAsset(uAssetPath, UNREAL_VERSION);
            DataTableExport myExport = (DataTableExport)openFile.Exports[0];
            table = myExport?.Table;
            if (table is null)
            {
                throw new InvalidDataException("Could not access data table. Please check you also have the corresponding .uexp file.");
            }
        }

        public List<StructPropertyData> Data => table.Data;

        private MessageList Messages
        {
            get
            {
                if (messages is null)
                {
                    messages = GetTrimmedData();
                }
                return messages;
            }
            set => messages = value;
        }

        public void ReadCsv(string inFilename) => Messages = MessageList.ReadCsv(inFilename);

        public void ReadJson(string inFilename) => Messages = MessageList.ReadJson(inFilename);

        public void ReplaceText()
        {
            var csvLookup = messages.Records.ToDictionary((entry) => entry.Identifier);
            foreach (var entry in Data)
            {
                if (entry.Value[1] is StrPropertyData strData)
                {
                    string targetValue = csvLookup[entry.Name.ToString()].Target;
                    if (!string.IsNullOrWhiteSpace(targetValue))
                    {
                        strData.Value = new FString(targetValue);
                    }
                }
            }
        }

        public void WriteUAssetToFile(string outputPath)
        {
            openFile.Write(outputPath);
        }

        public void WriteCsv(string outFilename) => Messages.WriteCsv(outFilename);

        public void WriteJson(string outFilename) => Messages.WriteJson(outFilename);

        private MessageList GetTrimmedData()
        {
            return new MessageList(Data.Select((entry) => new CSVClass(entry)));
        }
    }
}