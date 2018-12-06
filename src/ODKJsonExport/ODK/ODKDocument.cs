using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODKJsonExport.ODK
{
    public class ODKDocument
    {
        public string Name { get; set; }
        public int MyProperty { get; set; }
        public Dictionary<string,Dictionary<string, ODKObjectType>> TypesFields { get; set; }
        public ODKDocument()
        {
            TypesFields = new Dictionary<string, Dictionary<string, ODKObjectType>>();
        }
    }
}
