using ODKJsonExport.ODK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODKJsonExport.CSVExpress
{
    public class CSVDocumentCollection
    {
        private readonly string dir;
        Dictionary<string, CSVDocument> Documents;
        public CSVDocumentCollection(string dir)
        {
            Documents = new Dictionary<string, CSVDocument>();
            this.dir = dir;
        }
        public void AddDocument(string name, Dictionary<string, ODKObjectType> defenition)
        {
            if (!Documents.ContainsKey(name))
            {
                string fn = name == "" ? "Main" : name;
                Documents.Add(name, new CSVDocument(Path.Combine(dir, $"{fn}.csv"), defenition));
            }
        }
        public CSVDocument GetDocument(string name)
        {
            if (Documents.ContainsKey(name))
                return Documents[name];
            return null;
        }

        internal void Close()
        {
            var keys = Documents.Keys.ToList();
            foreach (var key in keys)
            {
                Documents[key].Dispose();
                Documents[key] = null;
            }
            Documents.Clear();
        }


        //csv.WriteField
    }
}
