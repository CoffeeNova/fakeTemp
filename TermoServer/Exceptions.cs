using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly.TermoServer
{

    [Serializable]
    public class GrainbarProcessNotExistException : Exception
    {
        public GrainbarProcessNotExistException() { }
        public GrainbarProcessNotExistException(string message) : base(message) { }
        public GrainbarProcessNotExistException(string message, Exception inner) : base(message, inner) { }
        protected GrainbarProcessNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class GrainbarMainWindowNotExistException : Exception
    {
        public GrainbarMainWindowNotExistException() { }
        public GrainbarMainWindowNotExistException(string message) : base(message) { }
        public GrainbarMainWindowNotExistException(string message, Exception inner) : base(message, inner) { }
        protected GrainbarMainWindowNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
