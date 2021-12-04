using System;

namespace SteamAuthCore
{
    public class ConfirmationModel
    {
        public ConfirmationModel(UInt64 id, UInt64 key, int type, UInt64 creator)
        {
            Id = id;
            Key = key;
            IntType = type;
            Creator = creator;

            //Do a switch simply because we're not 100% certain of all the possible types.
            ConfType = type switch
            {
                1 => ConfirmationType.GenericConfirmation,
                2 => ConfirmationType.Trade,
                3 => ConfirmationType.MarketSellTransaction,
                _ => ConfirmationType.Unknown
            };
        }

        public enum ConfirmationType
        {
            GenericConfirmation,
            Trade,
            MarketSellTransaction,
            Unknown
        }

        /// <summary>
        /// The ID of this confirmation
        /// </summary>
        public UInt64 Id { get; }

        /// <summary>
        /// The unique key used to act upon this confirmation.
        /// </summary>
        public UInt64 Key { get; }

        /// <summary>
        /// The value of the data-type HTML attribute returned for this contribution.
        /// </summary>
        public int IntType { get; }

        /// <summary>
        /// Represents either the Trade Offer ID or market transaction ID that caused this confirmation to be created.
        /// </summary>
        public UInt64 Creator { get; }

        /// <summary>
        /// The type of this confirmation.
        /// </summary>
        public ConfirmationType ConfType { get; }
    }
}
