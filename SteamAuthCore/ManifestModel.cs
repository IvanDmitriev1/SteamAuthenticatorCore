using System.Collections.ObjectModel;
using Newtonsoft.Json;

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

        [JsonProperty("encrypted")]
        public bool Encrypted { get; set; } = false;

        [JsonProperty("first_run")]
        public bool FirstRun { get; set; }

        [JsonProperty("accounts")]
        public ObservableCollection<SteamGuardAccount> Accounts { get; set; } = new();

        [JsonProperty("periodic_checking")]
        public bool PeriodicChecking { get; set; } = false;

        [JsonProperty("periodic_checking_interval")]
        public int PeriodicCheckingInterval { get; set; } = 10;

        [JsonProperty("periodic_checking_checkall")]
        public bool CheckAllAccounts { get; set; } = false;

        [JsonProperty("auto_confirm_market_transactions")]
        public bool AutoConfirmMarketTransactions { get; set; } = false;

        [JsonProperty("auto_confirm_trades")]
        public bool AutoConfirmTrades { get; set; } = false;

    }
}
