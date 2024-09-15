using AccountTransferApp.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Logging;

namespace AccountTransferApp.Services
{
    public class BankService
    {
        private readonly string _connectionString;  
        private readonly ILogger<BankService> _logger;

        public BankService(string connectionString, ILogger<BankService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public decimal GetAccountBalance(int accountId)
        {
            // Implementation to retrieve the balance of an account
            // This is just a placeholder implementation
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT Balance FROM Accounts WHERE AccountID = @AccountID", connection);
                command.Parameters.AddWithValue("@AccountID", accountId);
                connection.Open();
                return (decimal)command.ExecuteScalar();
            }
        }

        public List<Client> GetClients()
        {
            List<Client> clients = new List<Client>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT ClientID, Name FROM Clients", conn))  // Query to fetch clients
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Client client = new Client
                                {
                                    ClientID = (int)reader["ClientID"],
                                    Name = reader["Name"].ToString()
                                };
                                clients.Add(client);
                            }
                        }
                    }
                }

                _logger.LogInformation($"Successfully fetched {clients.Count} clients.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching clients.");
                return new List<Client>();  // Return empty list in case of error
            }

            return clients;  // Return the list of clients
        }


        // 1. GetClientAccounts: Calls the stored procedure sp_GetClientAccounts
        public List<Account> GetClientAccounts(int clientId)
        {
            List<Account> accounts = new List<Account>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetClientAccounts", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClientID", clientId);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Account account = new Account
                                {
                                    AccountID = (int)reader["AccountID"],
                                    AccountNumber = reader["AccountNumber"].ToString(),
                                    Balance = (decimal)reader["Balance"]
                                };
                                accounts.Add(account);
                            }
                        }
                    }
                }

                _logger.LogInformation($"Successfully fetched {accounts.Count} accounts for ClientID: {clientId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching accounts for ClientID: {clientId}");
                return new List<Account>();  // Return empty list in case of error
            }

            return accounts;  // Return the list of accounts
        }

        // 2. TransferAmount: Logic to call stored procedure sp_TransferAmount
        public void TransferAmount(int fromAccountId, int toAccountId, decimal amount)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_TransferAmount", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FromAccountID", fromAccountId);
                        cmd.Parameters.AddWithValue("@ToAccountID", toAccountId);
                        cmd.Parameters.AddWithValue("@Amount", amount);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                _logger.LogInformation($"Successfully transferred {amount:C} from Account {fromAccountId} to Account {toAccountId}");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000)  // Custom RAISERROR in SQL
                {
                    _logger.LogError(ex, "Transfer failed due to insufficient balance.");
                    throw new InvalidOperationException("Insufficient balance in the sender’s account.");
                }
                else
                {
                    _logger.LogError(ex, "SQL error occurred during the transfer.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during the transfer.");
                throw;
            }
        }

        // 3. GetUpdatedBalances: Calls the stored procedure sp_GetUpdatedBalances
        public List<Account> GetUpdatedBalances(int fromAccountId, int toAccountId)
        {
            List<Account> accounts = new List<Account>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetUpdatedBalances", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FromAccountID", fromAccountId);
                        cmd.Parameters.AddWithValue("@ToAccountID", toAccountId);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Account account = new Account
                                {
                                    AccountID = (int)reader["AccountID"],
                                    Balance = (decimal)reader["Balance"]
                                };
                                accounts.Add(account);
                            }
                        }
                    }
                }

                _logger.LogInformation($"Successfully fetched updated balances for accounts {fromAccountId} and {toAccountId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching updated balances for accounts {fromAccountId} and {toAccountId}");
                return new List<Account>();  // Return empty list in case of error
            }

            return accounts;  // Return the list of updated accounts
        }
    }

}
