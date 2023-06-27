using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using PokerFrontend.Infrastructure.Models;

namespace PokerFrontend.Business
{
    public class ReadSaveSettingsBusinessHandler
    {
        private string rangesFile;
        private string generalFile;
        public ReadSaveSettingsBusinessHandler()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Data\";
            System.IO.FileInfo file = new System.IO.FileInfo(folder);
            file.Directory?.Create();
            
            generalFile = @$"{folder}General.txt";
            rangesFile = @$"{folder}Ranges.txt";
            if (!File.Exists(rangesFile))
            {
                File.AppendAllText(rangesFile, "");
            }
            if (!File.Exists(generalFile))
            {
                File.AppendAllText(generalFile, "");
            }
        }


        public PreFlopRange GetPreFlopRangeById(Guid id)
        {
            var data = File.ReadAllText(rangesFile);
            var ranges = String.IsNullOrEmpty(data) ? new List<PreFlopRange>() : JsonSerializer.Deserialize<List<PreFlopRange>>(data);

            var range = ranges?.FirstOrDefault(x => x.Id == id);
            if (range == null)
            {
                throw new KeyNotFoundException();
            }
            return range;
        }

        public IEnumerable<PreFlopRange> GetPreFlopRanges()
        {
            var data = File.ReadAllText(rangesFile);
            var ranges = String.IsNullOrEmpty(data) ? new List<PreFlopRange>() : JsonSerializer.Deserialize<List<PreFlopRange>>(data);


            if (ranges == null)
            {
                throw new KeyNotFoundException();
            }
            return ranges;
        }

        public void SavePreFlopRange(PreFlopRange range)
        {
            var data = File.ReadAllText(rangesFile);
            var ranges = String.IsNullOrEmpty(data) ? new List<PreFlopRange>() : JsonSerializer.Deserialize<List<PreFlopRange>>(data);
            var dataRange = ranges?.FirstOrDefault(x => x.Id == range.Id);
            if (dataRange != null)
            {
                range = dataRange;
            }
            else
            {
                ranges.Add(range);
            }

            var resultString = JsonSerializer.Serialize(ranges);
            File.WriteAllText(rangesFile,resultString);
        }


    }
}
