using System;

// Unicode chars convert for Windows 1257 and 775 codepages

namespace Dotnet
{
    public static class Uni
    {
        //https://en.wikipedia.org/wiki/Specials_(Unicode_block)

        public const char REPLACEMENT_CHARACTER = '\ufffd'; // � U+FFFD  0xFDFF , select cast(0xFDFF as nvarchar(1))

        public static bool IsBrokenString(this string str) { return str.IndexOf(REPLACEMENT_CHARACTER) >= 0; }

        public static bool AnyWin1257Char(string str) { return Uni.AnyLTChar(str) || Uni.AnyLVChar(str) || Uni.AnyEEChar(str); }

        #region Ansi any chars

        // all: ąčęėįšųūž | ĄČĘĖĮŠŲŪŽ | āēģķļņ | ĀĒĢĶĻŅ | õäöü | ÕÄÖÜ
        // select cast('ąčęėįšųūž | ĄČĘĖĮŠŲŪŽ | āēģķļņ | ĀĒĢĶĻŅ | õäöü | ÕÄÖÜ' as varbinary(50))
        //        0xE0E8E6EBE1F0F8FBFE C0C8C6CBC1D0D8DBDE  E2E7ECEDEFF2 C2C7CCCDCFD2 F5E4F6FC D5

        // LT: ąčęėįšųūž | ĄČĘĖĮŠŲŪŽ
        public static bool AnyLTChar(string str) { return str.AnyInChars(CharsLT_Lower) || str.AnyInChars(CharsLT_Upper); }

        #region Map

        public static char[] CharsLT_Lower = new char[] { 'ą', 'č', 'ę', 'ė', 'į', 'š', 'ų', 'ū', 'ž' };
        public static char[] CharsLT_Upper = new char[] { 'Ą', 'Č', 'Ę', 'Ė', 'Į', 'Š', 'Ų', 'Ū', 'Ž' };
        public static char[] CharsLV_Lower = new char[] { 'ā', 'ē', 'ģ', 'ķ', 'ļ', 'ņ' };
        public static char[] CharsLV_Upper = new char[] { 'Ā', 'Ē', 'Ģ', 'Ķ', 'Ļ', 'Ņ' };
        public static char[] CharsEE_Lower = new char[] { 'õ', 'ä', 'ö', 'ü' };
        public static char[] CharsEE_Upper = new char[] { 'Õ', 'Ä', 'Ö', 'Ü' };

        // "Raktiniai" Ansi dekodavimo klaidu simboliai
        public static char[] Chars_OemFailed = new char[] { 'å', 'ü', 'ü', '½' };

        // LT: ąčęėįšųūž | ĄČĘĖĮŠŲŪŽ
        //      133 ->  ą  190 -> ž
        // LT  ą ę ė į ų ū + ž
        //     ąč        ę   ėį      šų      ūž 
        // 196: 133 141 153 151 175 161 179 171 190


        // LV āēģķļņ | ĀĒĢĶĻŅ
        //    129 147 171
        // LV  ā ē ī ū č ģ ķ ļ ņ | š ž
        //     Ā Ē Ī Ū Č Ģ Ķ Ļ Ņ | Š Ž
        //     129 147 163 183 188 134 ? ?

        // 195: EE  õäöü | ÕÄÖÜ ŠŽ šž
        //          181 164 182 188
        #endregion

        public static bool AnyLVChar(string str) { return str.AnyInChars(CharsLV_Lower) || str.AnyInChars(CharsLV_Upper); }

        public static bool AnyEEChar(string str) { return str.AnyInChars(CharsEE_Lower) || str.AnyInChars(CharsEE_Upper); }

        #endregion

        #region Oem

        // ü å ü ½  | select cast('┼ ü å ü ½' as binary(12))
        // ĻŠŃŅÓŌÕÖ×Ų
        // 0x3F FC E5 FC BD

        public static bool AnyOemChar(string str)
        {
            if (str.IndexOf(REPLACEMENT_CHARACTER) >= 0)    // jei Unicode convert klaida, tada ne Oem
                return false;           
            
            // symbols: new char[] { "å", 'ü', 'ü', '½' };
            return AnyInChars(str, Chars_OemFailed);
        }

        #endregion

        // Any character in anyOf was found; -1 if no character in anyOf was found.
        public static bool AnyInChars(this string str, char[] anyOf)
        {
            if (str == null || str.Length == 0 || anyOf.Length == 0)
                return false;
            return str.IndexOfAny(anyOf) >= 0;
        }
    }
}
