using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfxParser.Models
{
    public  class OfxTransaction
    {
        public string TRNTYPE { get; set; }
        public double  TRNAMT { get; set; }
        public DateTime DTPOSTED { get; set; }
        public string FITID { get; set; }
        public long CHECKNUM { get; set; }
        public string REFNUM {  get; set; }
        public string MEMO { get; set; }
    }
}
