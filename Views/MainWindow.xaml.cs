using AccountTransferApp.Services;
using AccountTransferApp.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Configuration;

namespace AccountTransferApp
{
    public partial class MainWindow : Window
    {
        private readonly BankService _bankService;

        public MainWindow()
        {
            InitializeComponent();

            // Define your connection string (replace with actual connection string)
            string connectionString = ConfigurationManager.ConnectionStrings["MyDatabaseConnectionString"].ConnectionString;

            // Use a NullLogger if you don't have a real logger in place
            ILogger<BankService> logger = NullLogger<BankService>.Instance;

            _bankService = new BankService(connectionString, logger);
            DataContext = new TransferViewModel(_bankService);
        }
    }
}
