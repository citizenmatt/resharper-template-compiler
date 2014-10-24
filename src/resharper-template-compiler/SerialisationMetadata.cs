using System;
using System.Text;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public static class SerialisationMetadata
    {
        private const int BytesInGuid = 16;

        public const string EntryIndexedValue = "@EntryIndexedValue";
        public const string EntryValue = "@EntryValue";
        public const string KeyIndexDefined = "@KeyIndexDefined";

        public static string FormatKey(string key)
        {
            return "=" + SerialiseKey(key);
        }

        private const char EscapeCharacter = '_';

        private static string SerialiseKey(string key)
        {
            var sb = new StringBuilder();
            foreach (var ch in key)
            {
                if (!IsLetThruChar(ch))
                {
                    unchecked
                    {
                        sb.Append(EscapeCharacter);

                        uint chData = ch;
                        for (int a = 3; a >= 0; a--)
                        {
                            uint q = (chData >> (a << 2)) & 0xF;
                            char chToAppend = (char)(q < 10 ? (q + '0') : (q - 10 + 'A'));
                            sb.Append(chToAppend);
                        }
                    }
                }
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }

        private static bool IsLetThruChar(char ch)
        {
            return ((ch >= '0') && (ch <= '9')) || ((ch >= 'A') && (ch <= 'Z')) || ((ch >= 'a') && (ch <= 'z'));
        }

        public static string ParseKey(string key)
        {
            return key.StartsWith("=") ? key.Substring(1) : key;
        }

        public static string FormatGuid(Guid guid)
        {
            // Copied from GuidIndex.ToString()
            var data = guid.ToByteArray();
            var chars = new char[BytesInGuid * 2];
            unchecked
            {
                for (int a = 0, b = 0; a < BytesInGuid; a++, b += 2)
                {
                    uint value = ((uint)data[a] >> 4) & 0xF;
                    chars[b] = (char)(value < 10 ? (value + '0') : (value - 10 + 'A'));

                    value = ((uint)data[a]) & 0xF;
                    chars[b + 1] = (char)(value < 10 ? (value + '0') : (value - 10 + 'A'));
                }
            }
            return new string(chars);
        }

        public static Guid ParseGuid(string guid)
        {
            // Copied from GuidIndex.Parse
            var data = new byte[BytesInGuid];
            unchecked
            {
                bool isGotUpper = false; // State machine state: if we already gut upper half of the byte and are waiting for the lower
                uint dwInProgress = 0; // Currently composed byte (upper half if isGotUpper)
                int nIndexInData = 0; // Write pos

                foreach (char ch in guid)
                {
                    if (nIndexInData >= BytesInGuid)
                        break; // Done, string has some extra chars which we ignore

                    // Current char to a digit
                    uint digit = ch;
                    var thisvalue = (uint)(-1);
                    if ((digit >= '0') && (digit <= '9'))
                        thisvalue = digit - '0';
                    else if ((digit >= 'A') && (digit <= 'F'))
                        thisvalue = digit - 'A' + 10;
                    else if ((digit >= 'a') && (digit <= 'f'))
                        thisvalue = digit - 'a' + 10;

                    // So, digit?
                    if (thisvalue != (uint)(-1))
                    {
                        // Compose (either lower or upper)
                        dwInProgress |= thisvalue;
                        if (isGotUpper)
                        {
                            // Already had upper and now thus added a lower => byte complete, submit and reset
                            data[nIndexInData++] = (byte)dwInProgress;
                            isGotUpper = false;
                            dwInProgress = 0;
                        }
                        else
                        {
                            // Had no upper => what we just found becomes an upper
                            dwInProgress <<= 4;
                            isGotUpper = true;
                        }
                    }
                }
            }

            return new Guid(data);
        }
    }
}