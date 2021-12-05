using System;

namespace WpfHelper.MarkupExtension
{
    public class EnumBindingSourceExtension : System.Windows.Markup.MarkupExtension
    {
        public Type EnumType { get; }

        public EnumBindingSourceExtension(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Enum type must be Enum", nameof(EnumBindingSourceExtension));

            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(EnumType);
        }
    }
}
