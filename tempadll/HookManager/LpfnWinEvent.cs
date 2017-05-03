using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gma.UserActivityMonitor
{
    public class LpfnWinEvent
    {
        public IntPtr hWinEventHook { get; set; }
        public int iEvent { get; set; }
        public IntPtr hWnd { get; set; }
        public int idObject { get; set; }
        public int idChild { get; set; }
        public int dwEventThread { get; set; }
        public int dwmsEventTime { get; set; }
    }
}
