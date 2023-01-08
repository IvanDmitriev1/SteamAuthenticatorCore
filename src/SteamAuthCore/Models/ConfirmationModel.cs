using System;
using AngleSharp.Dom;

namespace SteamAuthCore.Models;

public class ConfirmationModel
{
    public ConfirmationModel(in ReadOnlySpan<UInt64> attributesValue, string imageSource, IHtmlCollection<IElement> collection)
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
        ItemName = collection[0].TextContent.Remove(0, collection[0].TextContent.IndexOf('-') + 1).Trim();
        Description = collection[1].TextContent.Trim();
        Time = collection[2].TextContent;
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