using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ODKJsonExport.ODK;
using ODKJsonExport.CSVExpress;

namespace ODKJsonExport
{
    public partial class Form1 : Form
    {
        const string DEFAULT_TYPE_TITLE = "(Default)";
        const string MULTI_VAL_SEPERATOR = " ";

        ODKDocument currentDoc = new ODKDocument();
        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            currentDoc = new ODKDocument();
            string fn = txtFilename.Text;
            using (StreamReader file = File.OpenText(fn))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JToken o = JToken.ReadFrom(reader);
                if (o is JArray)
                {
                    foreach (JObject jo in ((JArray)o))
                    {
                        ODKUtils.AnalyseObject(currentDoc, jo, "");
                    }
                    lstTypes.Items.Clear();
                    foreach (var typ in currentDoc.TypesFields)
                    {
                        lstTypes.Items.Add(typ.Key == "" ? DEFAULT_TYPE_TITLE : typ.Key);
                    }
                }
                else
                {
                    throw new Exception("Not valid ODK exportion");
                }
            }


        }


        private void lstTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var s = (string)lstTypes.SelectedItem;
            if (s == DEFAULT_TYPE_TITLE) s = "";
            var type = currentDoc.TypesFields[s];
            lstFields.Items.Clear();
            foreach (var field in type)
            {
                lstFields.Items.Add($"{field.Key} : {field.Value.ToString()}");
            }
        }

        private void btnFill_Click(object sender, EventArgs e)
        {
            if(currentDoc== null || currentDoc.TypesFields==null || currentDoc.TypesFields.Count ==0)
            {
                MessageBox.Show("File not analyzed yet!", "Error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string fn = txtFilename.Text;
            string outPath = Path.GetDirectoryName(fn);
            outPath = Path.Combine(outPath, $"{Path.GetFileNameWithoutExtension(fn)}_csvs");
            CSVDocumentCollection docCollection = new CSVDocumentCollection(outPath);

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            foreach (var type in currentDoc.TypesFields)
                docCollection.AddDocument(type.Key, type.Value);

            using (StreamReader file = File.OpenText(fn))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JToken o = JToken.ReadFrom(reader);
                if (o is JArray)
                {
                    int i = 1;
                    foreach (JObject jo in ((JArray)o))
                    {
                        string instanceId = jo["instanceID"].Value<string>();
                        ProcessObject(currentDoc.TypesFields, docCollection, jo, "", instanceId, i++, false);
                    }
                    docCollection.Close();
                }
                else
                {
                    throw new Exception("Not valid ODK exportion");
                }

            }


        }

        private void ProcessObject(Dictionary<string, Dictionary<string, ODKObjectType>> typesFields,
            CSVDocumentCollection docCollection,
            JObject jo,
            string currentType,
            string instanceId,
            int index,
            bool isSub
        )
        {
            List<object> record = new List<object>();
            if (isSub) record.Add(instanceId);
            record.Add(index);
            foreach (var prop in jo.Properties())
            {
                var t = typesFields[currentType][prop.Name];
                if (prop.Value.Type == JTokenType.Null)
                {
                    record.Add("");
                    continue;
                }

                switch (t)
                {
                    case ODKObjectType.TextualValue:
                        record.Add(prop.Value.Value<string>());
                        break;
                    case ODKObjectType.MultiValue:
                        record.Add(JoinMultiValue(prop.Value as JArray));
                        break;
                    case ODKObjectType.File:
                        record.Add(ProcessFile(prop.Value));
                        break;
                    case ODKObjectType.Repeat:
                        if (prop.Value.Type != JTokenType.Null)
                        {
                            int i = 1;
                            foreach (var rec in (prop.Value as JArray))
                            {
                                ProcessObject(typesFields, docCollection, rec as JObject, ODK.ODKUtils.ExtractTypeName(prop.Name), instanceId, i++, true);
                            }
                        }
                        break;
                    case ODKObjectType.Unknown:
                        record.Add("");
                        break;
                    default:
                        break;
                }

            }
            var doc = docCollection.GetDocument(currentType);
            doc.WriteRecord(record);

        }

        private object ProcessFile(JToken value)
        {
            if (value.Type != JTokenType.Null)
                return value["url"].Value<string>();
            return "";
        }

        private object JoinMultiValue(JArray jArray)
        {
            string retVal = "";
            foreach (var v in jArray)
            {
                if (retVal != "")
                    retVal += MULTI_VAL_SEPERATOR;
                retVal += v;
            }
            return retVal;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Files|*.json";
            ofd.ShowDialog();
            txtFilename.Text = ofd.FileName;
        }
    }
}
