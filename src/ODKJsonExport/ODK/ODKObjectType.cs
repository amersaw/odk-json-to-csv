using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODKJsonExport.ODK
{
    public enum ODKObjectType
    {

        TextualValue = 1,
        MultiValue = 2,
        File = 3,
        Repeat = 5,
        Unknown = 99
    }
}
