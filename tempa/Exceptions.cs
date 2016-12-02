using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa.Exceptions
{
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


    [Serializable]
    public class PlotDataException : Exception
    {
        public PlotDataException() { }
        public PlotDataException(string message) : base(message) { }
        public PlotDataException(string message, Exception inner) : base(message, inner) { }
        protected PlotDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
