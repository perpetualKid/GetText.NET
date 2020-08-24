using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetText.Extractor.Template
{
    [Flags]
    public enum MessageFlags
    {
        None =          0x0,
        Fuzzy =         0x1,
        CSharpFormat =  0x2,
    }
}
