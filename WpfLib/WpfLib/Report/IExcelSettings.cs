using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Report.Excel
{
    public interface IExcelSettings
    {
        bool AutoFilter { get; set; }
        bool AligmentTop { get; set; }
    }
}
