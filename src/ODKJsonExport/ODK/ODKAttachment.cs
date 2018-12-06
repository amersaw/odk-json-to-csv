using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODKJsonExport.ODK
{
    public class ODKAttachment
    {
        public string Filename { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string InstanceId { get; set; }
        public string Path { get; set; }

    }
}
