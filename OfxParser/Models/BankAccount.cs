using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfxParser.Models
{
    public class BankAccount
    {
        public int BANKID { get; set; }
        public string BRANCHID { get; set; }
        public string ACCTID { get; set; }
        public string ACCTTYPE { get; set; }
    }
}
