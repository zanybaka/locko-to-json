using System;

namespace locko_to_json.BitwardenDto
{
    public static class BitwardenHelper
    {
        public static Field ConvertToBitwardenField(string name, string value)
        {
            Field field = new Field();
            field.name  = name;
            field.value = value;
            field.type  = 0; // string
            string loweredValue = value?.ToLowerInvariant() ?? "";
            string loweredName  = name?.ToLowerInvariant() ?? "";

            if (loweredValue == "false" || loweredValue == "true")
            {
                field.type = 2; // boolean
            }

            if (loweredName.Contains("pass") || loweredName.Contains("code") || loweredName == "pin")
            {
                field.type = 1; // hidden
            }

            field.value = TruncateToMaxNumberOfSymbols(name, value);

            return field;
        }

        public static string TruncateToMaxNumberOfSymbols(string name, string value)
        {
            if (value != null && value.Length > 650) // max encrypted length is 1000. 650 is an approximate unencrypted max length.
            {
                Console.WriteLine($"'{name}'s value has been truncated to 1000 symbols: {value}");
                return value.Substring(0, 650);
            }

            return value;
        }
    }
}