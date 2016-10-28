using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa.Exceptions
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

    public class PlotDataException : Exception
    {
        public PlotDataException(string message) : base(message) { }

        public PlotDataException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class TermometerBuildException : Exception
    {
        public TermometerBuildException(string message) : base(message) { }

        public TermometerBuildException(string message, Exception innerException) : base(message, innerException) { }
    }


    [Serializable]
    public class FolderException : Exception
    {
        public FolderException() { }
        public FolderException(string message) : base(message) { }
        public FolderException(string message, Exception inner) : base(message, inner) { }
        protected FolderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
