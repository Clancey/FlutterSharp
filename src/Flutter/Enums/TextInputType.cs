// FlutterSharp Manual Implementation
// TextInputType enum

using System;

namespace Flutter.Services
{
    /// <summary>
    /// The type of information for which to optimize the text input control.
    ///
    /// On Android, behavior may vary across device and keyboard provider.
    ///
    /// This class stays as close to Enum interface as possible, and provides
    /// factory constructors for all types to ensure consistency.
    /// </summary>
    public enum TextInputType
    {
        /// <summary>
        /// Optimize for textual information.
        /// Requests the default platform keyboard.
        /// </summary>
        Text = 0,

        /// <summary>
        /// Optimize for multiline textual information.
        /// Requests the default platform keyboard, but accepts newlines when
        /// the enter key is pressed.
        /// </summary>
        Multiline = 1,

        /// <summary>
        /// Optimize for unsigned numerical information without a decimal point.
        /// Requests a default keyboard with ready access to the number keys.
        /// </summary>
        Number = 2,

        /// <summary>
        /// Optimize for telephone numbers.
        /// Requests a keyboard with ready access to the number keys, "*" and "#".
        /// </summary>
        Phone = 3,

        /// <summary>
        /// Optimize for date and time information.
        /// On iOS, requests the default keyboard.
        /// On Android, requests a keyboard with ready access to the number keys,
        /// ":", and "-".
        /// </summary>
        Datetime = 4,

        /// <summary>
        /// Optimize for email addresses.
        /// Requests a keyboard with ready access to the "@" and "." keys.
        /// </summary>
        EmailAddress = 5,

        /// <summary>
        /// Optimize for URLs.
        /// Requests a keyboard with ready access to the "/" and "." keys.
        /// </summary>
        Url = 6,

        /// <summary>
        /// Optimize for passwords that are visible to the user.
        /// Requests a keyboard with ready access to both letters and numbers.
        /// </summary>
        VisiblePassword = 7,

        /// <summary>
        /// Optimize for a person's name.
        /// On iOS, requests the Name Phonetic keyboard.
        /// On Android, requests a keyboard optimized for names.
        /// </summary>
        Name = 8,

        /// <summary>
        /// Optimize for a postal mailing address.
        /// On iOS, requests the default keyboard.
        /// On Android, requests a keyboard optimized for addresses,
        /// with ready access to the number keys.
        /// </summary>
        StreetAddress = 9,

        /// <summary>
        /// Prevents the OS from inferring the type of input.
        /// Requests a keyboard without any special behavior.
        /// </summary>
        None = 10
    }

    /// <summary>
    /// Extension methods for TextInputType enum.
    /// </summary>
    public static class TextInputTypeExtensions
    {
        /// <summary>
        /// Converts the enum to its Dart integer representation.
        /// </summary>
        public static int ToDartInt(this TextInputType value)
        {
            return (int)value;
        }

        /// <summary>
        /// Converts the enum to its Dart string representation.
        /// </summary>
        public static string ToDartString(this TextInputType value)
        {
            return value switch
            {
                TextInputType.Text => "text",
                TextInputType.Multiline => "multiline",
                TextInputType.Number => "number",
                TextInputType.Phone => "phone",
                TextInputType.Datetime => "datetime",
                TextInputType.EmailAddress => "emailAddress",
                TextInputType.Url => "url",
                TextInputType.VisiblePassword => "visiblePassword",
                TextInputType.Name => "name",
                TextInputType.StreetAddress => "streetAddress",
                TextInputType.None => "none",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        /// <summary>
        /// Converts from a Dart integer representation to the enum.
        /// </summary>
        public static TextInputType FromDartInt(int value)
        {
            return (TextInputType)value;
        }
    }
}
