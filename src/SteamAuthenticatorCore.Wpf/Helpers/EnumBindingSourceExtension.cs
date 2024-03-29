﻿using System.Windows.Markup;

namespace SteamAuthenticatorCore.Desktop.Helpers;

[MarkupExtensionReturnType(typeof(Array))]
internal class EnumBindingSourceExtension : System.Windows.Markup.MarkupExtension
{
    public int Offset { get; set; }
    public Type EnumType { get; }

    public EnumBindingSourceExtension(Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("The type must be an Enum", nameof(EnumBindingSourceExtension));

        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var arr = Enum.GetValues(EnumType);
        if (Offset <= 0)
            return arr;

        Array.Reverse(arr);
        var newArr = new object[arr.Length - Offset];
        Array.Copy(arr, newArr, arr.Length - Offset);

        return newArr;
    }
}