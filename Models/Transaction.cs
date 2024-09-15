using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountTransferApp.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int FromAccountID { get; set; }
        public int ToAccountID { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
