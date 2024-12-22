using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfxParser.Core;

namespace OfxParser.Models
{
    public class OfxData
    {
        
        private List<string> _warnings= [];
        private List<OfxTransaction> _transactions = [];
        public Bank FI { get; set; }
        public BankAccount BANKACCTFROM { get; set; }
        public DateTime DTSTART { get; set; }
        public DateTime DTEND { get; set; }
        public double BALAMT { get; set; }
        public DateTime DTASOF { get; set; }
        public IReadOnlyCollection<string> ImportWarnings { get { return _warnings; } }
        public IReadOnlyCollection<OfxTransaction> STMTTRN { get { return _transactions; } }

        public void AddWarning(string warning) { _warnings.Add(warning); }
        public void AddTransaction(OfxTransaction transaction) { _transactions.Add(transaction); }
    }
}
