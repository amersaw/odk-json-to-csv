using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODKJsonExport.ODK
{
    public static class ODKUtils
    {

        public static void AnalyseObject(ODKDocument doc, JObject jo, string typeName)
        {
            foreach (var pr in jo.Properties())
            {
                var tp = DetermineType(pr.Value);
                if (!doc.TypesFields.ContainsKey(typeName))
                    doc.TypesFields.Add(typeName, new Dictionary<string, ODKObjectType>());
                var fields = doc.TypesFields[typeName];
                if (fields.ContainsKey(pr.Name))
                {
                    if (fields[pr.Name] != tp)
                    {
                        if (fields[pr.Name] == ODKObjectType.Unknown)
                            fields[pr.Name] = tp;
                    }
                }
                else
                    fields.Add(pr.Name, tp);
                if (fields[pr.Name] == ODKObjectType.Repeat && (pr.Value as JArray).Count > 0)
                {

                    string secTypeName = ExtractTypeName(pr.Name);
                    foreach (var item in pr.Value as JArray)
                    {
                        AnalyseObject(doc, item as JObject, secTypeName);

                    }
                }
            }
        }

        public static string ExtractTypeName(string name)
        {
            return name.Replace("_repeat", "");
        }

        public static ODKObjectType DetermineType(JToken token)
        {
            if (IsDirectValue(token.Type))
            {
                return ODKObjectType.TextualValue;
            }
            if (token.Type == JTokenType.Object)
            {
                JObject jobj = (JObject)token;
                var fileAttrs = new string[] { "filename", "type", "url" };
                var propNames = jobj.Properties().Select(p => p.Name);
                if (propNames.Count() == 3 && fileAttrs.All(a => propNames.Contains(a)))
                {
                    return ODKObjectType.File;
                }

            }
            if (token is JArray)
            {
                var arr = token as JArray;
                if (arr.Count == 0) //unknown
                    return ODKObjectType.Unknown;
                else
                {
                    if (arr[0].Type == JTokenType.Object) return ODKObjectType.Repeat;
                    else if (IsDirectValue(arr[0].Type)) return ODKObjectType.MultiValue;
                }
            }

            return ODKObjectType.Unknown;
        }
        public static bool IsDirectValue(JTokenType t)
        {
            var direcValues = new JTokenType[] {
                //JTokenType.Property       ,
                JTokenType.Integer        ,
                JTokenType.Float          ,
                JTokenType.String         ,
                JTokenType.Boolean        ,
                //JTokenType.Null           ,
                JTokenType.Date           ,
                JTokenType.Raw            ,
                JTokenType.Bytes          ,
                JTokenType.Guid           ,
                JTokenType.Uri            ,
                JTokenType.TimeSpan
            };
            return direcValues.Contains(t);
        }
    }
}
