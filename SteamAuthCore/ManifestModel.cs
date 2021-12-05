using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SteamAuthCore
{
    public class ManifestModel
    {
        public ManifestModel()
        {

        }

        public ManifestModel(bool firstRun = false)
        {
            FirstRun = firstRun;
        }

        public ManifestModel(ManifestModel model)
        {
            AutoConfirmMarketTransactions = model.AutoConfirmMarketTransactions;
            AutoConfirmTrades = model.AutoConfirmTrades;
            CheckAllAccounts = model.CheckAllAccounts;
            Encrypted = model.Encrypted;
            FirstRun = model.FirstRun;
            PeriodicChecking = model.PeriodicChecking;
            PeriodicCheckingInterval = model.PeriodicCheckingInterval;
        }

        #region Properties

        [JsonPropertyName("encrypted")]
        public bool Encrypted { get; set; } = false;

        [JsonPropertyName("first_run")]
        public bool FirstRun { get; set; }

        [JsonPropertyName("accounts")]
        public ObservableCollection<SteamGuardAccount>? Accounts { get; set; }

        [JsonPropertyName("periodic_checking")]
        public bool PeriodicChecking { get; set; } = false;

        [JsonPropertyName("periodic_checking_interval")]
        public int PeriodicCheckingInterval { get; set; } = 10;

        [JsonPropertyName("periodic_checking_checkall")]
        public bool CheckAllAccounts { get; set; } = false;

        [JsonPropertyName("auto_confirm_market_transactions")]
        public bool AutoConfirmMarketTransactions { get; set; } = false;

        [JsonPropertyName("auto_confirm_trades")]
        public bool AutoConfirmTrades { get; set; } = false;

        #endregion
    }
}
