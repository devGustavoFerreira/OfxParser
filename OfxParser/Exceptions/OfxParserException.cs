using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfxParser.Exceptions
{
    public class OFXParserException : Exception
    {
        public OFXParserException(string message) : base(message)
        {

        }
    }
}
