using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.NabiLog
{
    public interface INabiLog
    {
        void WriteLog(params object[] args);
    }
}
