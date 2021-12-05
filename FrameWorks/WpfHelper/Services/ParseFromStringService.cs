using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfHelper.Services
{
    public static class ParseFromStringService<T>
    {
        /// <summary>
        /// Uses KeyEventArgs and RoutedEventArgs to pass text
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <param name="setCustomValueWhenError"></param>
        /// <returns>Don't  forget to check output
        /// <example><code>if(result != default)</code></example>
        /// then assign value</returns>
        public static T? ParseFromTesBoxEventArgs(object? obj, in T defaultValue, in string? setCustomValueWhenError = null)
        {
            if (defaultValue is null) throw new ArgumentException("It cannot be null", nameof(defaultValue));

            object result;

            switch (obj)
            {
                case KeyEventArgs { Source: TextBox textBox }:
                    {
                        if (!Keyboard.IsKeyDown(Key.Enter) && !Keyboard.IsKeyDown(Key.Tab)) return default(T);

                        result = TexBoxParse(ref textBox, defaultValue, setCustomValueWhenError)!;
                        break;
                    }

                case RoutedEventArgs { Source: TextBox textBox1 }:
                    result = TexBoxParse(ref textBox1, defaultValue, setCustomValueWhenError)!;
                    break;
                default:
                    result = defaultValue;
                    break;
            }

            return (T)result;
        }

        /// <summary>
        /// Uses textBox to properly implement Parse method
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="defaultValue"></param>
        /// <param name="setCustomValueWhenError"></param>
        /// <returns></returns>
        public static T TexBoxParse(ref TextBox textBox, in T defaultValue, in string? setCustomValueWhenError = null)
        {
            string text = textBox.Text;

            object result = Parse(ref text, defaultValue, setCustomValueWhenError)!;

            if (!string.Equals(text, textBox.Text))
                textBox.Text = text;

            return (T)result;
        }

        /// <summary>
        /// Uses string to parse to T type
        /// </summary>
        /// <param name="text"></param>
        /// <param name="defaultValue"></param>
        /// <param name="customString"></param>
        /// <returns></returns>
        public static T Parse(ref string text, in T defaultValue, in string? customString = null)
        {
            if (defaultValue is null) throw new ArgumentException("It cannot be null", nameof(defaultValue));

            var tryParseMethod = typeof(T).GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(T).MakeByRefType() }, null);

            object?[] parameters = { text, null };
            object? boolValue = tryParseMethod?.Invoke(null, parameters);

            object result;
            if (boolValue is false)
            {
                text = customString ?? defaultValue.ToString()!;

                result = defaultValue;
                return (T)result;
            }

            result = (T)parameters[1]!;
            return (T)result;
        }
    }
}
