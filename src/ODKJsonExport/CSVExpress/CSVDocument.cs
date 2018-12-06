using CsvHelper;
using ODKJsonExport.ODK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODKJsonExport.CSVExpress
{
    public class CSVDocument : IDisposable
    {
        private readonly Dictionary<string, ODKObjectType> defenition;
        private TextWriter writer;
        private CsvWriter csv;


        public CSVDocument(string path, Dictionary<string, ODKObjectType> defenition)
        {
            writer = new StreamWriter(path, false, Encoding.UTF8);
            csv = new CsvWriter(writer);
            this.defenition = defenition;
            List<object> header = new List<object>();
            if (!defenition.ContainsKey("instanceID"))
                header.Add("instanceID");
            header.Add("#");
            foreach (var item in defenition)
            {
                if (item.Value == ODKObjectType.TextualValue || 
                    item.Value == ODKObjectType.MultiValue || 
                    item.Value == ODKObjectType.File ||
                    item.Value == ODKObjectType.Unknown
                )
                    header.Add(item.Key);
            }
            WriteRecord(header);
        }
        public void WriteRecord(List<object> record)
        {
            foreach (var item in record)
            {
                csv.WriteField(item);
            }
            csv.NextRecord();

        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }
    }
}
