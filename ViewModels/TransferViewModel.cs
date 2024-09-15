using AccountTransferApp.Models;
using AccountTransferApp.Services;
using AccountTransferApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AccountTransferApp.ViewModels
{
    public class TransferViewModel : INotifyPropertyChanged
    {
        private readonly BankService _bankService;
        private decimal _transferAmount;
        private string _errorMessage;
        private Client _selectedFirstClient;
        private Account _selectedFirstClientAccount;
        private Client _selectedSecondClient;
        private Account _selectedSecondClientAccount;

        // Properties for binding
        private List<Client> _clients;
        public List<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
                OnPropertyChanged(nameof(AvailableSecondClients)); // Notify that the available clients have changed
            }
        }

        private IEnumerable<Client> _filteredFirstClients;
        private IEnumerable<Client> _filteredSecondClients;

        public IEnumerable<Client> FilteredFirstClients
        {
            get => _filteredFirstClients;
            private set
            {
                _filteredFirstClients = value;
                OnPropertyChanged(nameof(FilteredFirstClients));
            }
        }

        public IEnumerable<Client> FilteredSecondClients
        {
            get => _filteredSecondClients;
            private set
            {
                _filteredSecondClients = value;
                OnPropertyChanged(nameof(FilteredSecondClients));
            }
        }

        public List<Account> FirstClientAccounts { get; set; }
        public List<Account> SecondClientAccounts { get; set; }

        private decimal _firstClientUpdatedBalance;
        private decimal _secondClientUpdatedBalance;

        public decimal FirstClientUpdatedBalance
        {
            get => _firstClientUpdatedBalance;
            set
            {
                if (_firstClientUpdatedBalance != value)
                {
                    _firstClientUpdatedBalance = value;
                    OnPropertyChanged(nameof(FirstClientUpdatedBalance));
                }
            }
        }

        public decimal SecondClientUpdatedBalance
        {
            get => _secondClientUpdatedBalance;
            set
            {
                if (_secondClientUpdatedBalance != value)
                {
                    _secondClientUpdatedBalance = value;
                    OnPropertyChanged(nameof(SecondClientUpdatedBalance));
                }
            }
        }

        public ICommand ResetCommand => new RelayCommand(Reset);

        private void Reset(object parameter)
        {
            // Reset properties
            SelectedFirstClient = null;
            SelectedFirstClientAccount = null;
            SelectedSecondClient = null;
            SelectedSecondClientAccount = null;
            TransferAmount = 0;
            ErrorMessage = string.Empty;

            // Reload client and account lists if needed
            LoadClients();
        }

        public Client SelectedFirstClient
        {
            get => _selectedFirstClient;
            set
            {
                if (_selectedFirstClient != value)
                {
                    _selectedFirstClient = value;
                    OnPropertyChanged(nameof(SelectedFirstClient));
                    UpdateFilteredClients();
                    LoadFirstClientAccounts();
                }
            }
        }

        public Account SelectedFirstClientAccount
        {
            get => _selectedFirstClientAccount;
            set
            {
                _selectedFirstClientAccount = value;
                OnPropertyChanged(nameof(SelectedFirstClientAccount));
            }
        }

        public Client SelectedSecondClient
        {
            get => _selectedSecondClient;
            set
            {
                if (_selectedSecondClient != value)
                {
                    _selectedSecondClient = value;
                    OnPropertyChanged(nameof(SelectedSecondClient));
                    UpdateFilteredClients();
                    LoadSecondClientAccounts();
                }
            }
        }

        public Account SelectedSecondClientAccount
        {
            get => _selectedSecondClientAccount;
            set
            {
                _selectedSecondClientAccount = value;
                OnPropertyChanged(nameof(SelectedSecondClientAccount));
            }
        }

        public decimal TransferAmount
        {
            get => _transferAmount;
            set
            {
                if (value <= 0)
                {
                    ErrorMessage = "Transfer amount must be greater than zero.";
                }
                else
                {
                    _transferAmount = value;
                }
                OnPropertyChanged(nameof(TransferAmount));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        // Constructor
        public TransferViewModel(BankService bankService)
        {
            _bankService = bankService;
            LoadClients();
        }

        // Load Clients
        private void LoadClients()
        {
            try
            {
                var clients = _bankService.GetClients();
                Clients = clients.ToList();
                UpdateFilteredClients();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading clients: {ex.Message}";
            }
        }

        private void UpdateFilteredClients()
        {
            FilteredFirstClients = Clients.Where(c => c != SelectedSecondClient);
            FilteredSecondClients = Clients.Where(c => c != SelectedFirstClient);
        }

        // Load Accounts for First Client
        private void LoadFirstClientAccounts()
        {
            if (SelectedFirstClient != null)
            {
                try
                {
                    FirstClientAccounts = _bankService.GetClientAccounts(SelectedFirstClient.ClientID);
                    OnPropertyChanged(nameof(FirstClientAccounts));
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error loading accounts for the first client: {ex.Message}";
                }
            }
        }

        // Load Accounts for Second Client
        private void LoadSecondClientAccounts()
        {
            if (SelectedSecondClient != null)
            {
                try
                {
                    SecondClientAccounts = _bankService.GetClientAccounts(SelectedSecondClient.ClientID);
                    OnPropertyChanged(nameof(SecondClientAccounts)); // Notify UI of the change
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error loading accounts for the second client: {ex.Message}";
                }
            }
            else
            {
                SecondClientAccounts = new List<Account>(); // Clear accounts if no client selected
                OnPropertyChanged(nameof(SecondClientAccounts));
            }
        }

        // Get available clients for the second ComboBox
        public List<Client> AvailableSecondClients
        {
            get
            {
                if (Clients == null) return new List<Client>();
                if (SelectedFirstClient == null) return Clients;
                return Clients.Where(c => c != SelectedFirstClient).ToList();
            }
        }

        // Command for executing the transfer
        public ICommand ExecuteTransferCommand => new RelayCommand(ExecuteTransfer, CanExecuteTransfer);

        private bool CanExecuteTransfer(object parameter)
        {
            // Ensure all required fields are selected and transfer amount is valid
            return SelectedFirstClientAccount != null && SelectedSecondClientAccount != null && TransferAmount > 0;
        }

        // Modify ExecuteTransfer to update balances
        public void ExecuteTransfer(object parameter)
        {
            if (SelectedFirstClientAccount == null || SelectedSecondClientAccount == null)
            {
                ErrorMessage = "Please select both accounts.";
                return;
            }

            // Open confirmation dialog
            var dialog = new TransferDialog
            {
                DataContext = this
            };
            var result = dialog.ShowDialog();

            if (result == true) // Check the DialogResult from the dialog
            {
                try
                {
                    _bankService.TransferAmount(SelectedFirstClientAccount.AccountID, SelectedSecondClientAccount.AccountID, TransferAmount);

                    // Update balances after successful transfer
                    FirstClientUpdatedBalance = _bankService.GetAccountBalance(SelectedFirstClientAccount.AccountID);
                    SecondClientUpdatedBalance = _bankService.GetAccountBalance(SelectedSecondClientAccount.AccountID);

                    ErrorMessage = "Transfer completed successfully.";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Transfer failed: {ex.Message}";
                }
            }
        }

        public ICommand ConfirmTransferCommand => new RelayCommand(ConfirmTransfer);
        public ICommand CancelCommand => new RelayCommand(CancelTransfer);

        private void ConfirmTransfer(object parameter)
        {
            // The ExecuteTransfer logic is already in place, so no need to duplicate
            ((Window)Application.Current.MainWindow).DialogResult = true; // Close dialog with "OK"
        }

        private void CancelTransfer(object parameter)
        {
            ((Window)Application.Current.MainWindow).DialogResult = false; // Close dialog with "Cancel"
        }

        public ICommand OpenTransferDialogCommand => new RelayCommand(OpenTransferDialog);
         
        private void OpenTransferDialog(object parameter)
        {
            var dialog = new TransferDialog
            {
                DataContext = this
            };
            dialog.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
