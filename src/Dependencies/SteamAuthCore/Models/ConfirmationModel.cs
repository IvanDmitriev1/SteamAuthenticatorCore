namespace SteamAuthCore.Models;

public class ConfirmationModel
{
    public ConfirmationModel(in ulong[] attributesValue, string imageSource, in string[] descriptionArray)
    {
        Id = attributesValue[0];
        Key = attributesValue[1];
        Creator = attributesValue[3];

        ConfType = attributesValue[2] switch
        {
            1 => ConfirmationType.GenericConfirmation,
            2 => ConfirmationType.Trade,
            3 => ConfirmationType.MarketSellTransaction,
            _ => ConfirmationType.Unknown
        };

        ImageSource = imageSource;

        var itemName = descriptionArray[0];
        itemName = descriptionArray[0].Remove(0, itemName.IndexOf('-') + 1);

        ItemName = itemName.Trim();
        Description = descriptionArray[1].Trim();
        Time = descriptionArray[2];
    }

    /// <summary>
    /// The ID of this confirmation
    /// </summary>
    public ulong Id { get; }

    /// <summary>
    /// The unique key used to act upon this confirmation.
    /// </summary>
    public ulong Key { get; }

    /// <summary>
    /// Represents either the Trade Offer ID or market transaction ID that caused this confirmation to be created.
    /// </summary>
    public ulong Creator { get; }

    /// <summary>
    /// The type of this confirmation.
    /// </summary>
    public ConfirmationType ConfType { get; }
    public string ImageSource { get; }
    public object? BitMapImage { get; set; }
    public string ItemName { get; }
    public string Description { get; }
    public string Time { get; }
}