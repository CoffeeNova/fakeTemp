using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempadll.Exceptions
{
        public class ReportFileException : Exception
        {
            public ReportFileException(string message) : base(message) { }

            public ReportFileException(string message, Exception innerException) : base(message, innerException) { }
        }

        public class WriteReportException : Exception
        {
            public WriteReportException(string message) : base(message) { }

            public WriteReportException(string message, Exception innerException) : base(message, innerException) { }
        }

        public class TermometerBuildException : Exception
        {
            public TermometerBuildException(string message) : base(message) { }

            public TermometerBuildException(string message, Exception innerException) : base(message, innerException) { }
        }
}
