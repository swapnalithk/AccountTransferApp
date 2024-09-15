using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountTransferApp.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public int ClientID { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
    }
}
