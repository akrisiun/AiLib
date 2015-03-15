using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Ai.Util
{
    public static class ClipboardWin32
    {

        const int CF_TEXT = 1;                    // const
        const int CLIPBOARD_MAXSIZE = 2000000;
        // 65520;      // 0x0FFF0

        [DllImport("user32")]
        public static extern int IsClipboardFormatAvailable(int wFormat);

        [DllImport("user32")]
        public static extern int OpenClipboard(int format);

        [DllImport("user32")]
        public static extern IntPtr CloseClipboard();

        [DllImport("user32")]
        public static extern IntPtr GetClipboardData(int format);

        [DllImport("kernel32.dll")]
        static extern UIntPtr GlobalSize(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int GlobalUnlock(IntPtr hMem);

        public static string GetPaste()
        {

            string strTxtData = "";

            if (0 == IsClipboardFormatAvailable(CF_TEXT)
                || 0 == OpenClipboard(0))
                return strTxtData;

            // Get the handle of the Global memory that contains the text HGLOBAL 
            IntPtr hData = GetClipboardData(CF_TEXT);

            const int CF_UNICODETEXT = 13;

            IntPtr hData2 = GetClipboardData(CF_UNICODETEXT);
            if (hData2 != IntPtr.Zero) hData = hData2;

            if (hData != IntPtr.Zero)
            {
                // Get the size of the data
                UInt32 dataSize = (UInt32)GlobalSize(hData);
                IntPtr lptstr = GlobalLock(hData);

                if (lptstr != IntPtr.Zero && dataSize > 0)
                {
                    // Allocate data and copy the data
                    if (dataSize > CLIPBOARD_MAXSIZE) dataSize = CLIPBOARD_MAXSIZE;
                    byte[] dataBytes = new byte[dataSize];

                    Marshal.Copy(lptstr, dataBytes, 0, (int)dataSize);
                    GlobalUnlock(hData);

                    if (hData2 != IntPtr.Zero)
                    {
                        byte[] utfWide = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, dataBytes);

                        // Fill UTF8 bytes inside UTF8 string
                        for (int i = 0; i < utfWide.Length; i++)
                        {
                            if (utfWide[i] == 196 || utfWide[i] == 197)  // LT/LV
                            {
                                byte char1 = utfWide[i + 1];
                                // LT: ąčęėįšųūž | ĄČĘĖĮŠŲŪŽ
                                //      133 ->  ą  190 -> ž
                                if (char1 == 133 && utfWide[i] == 196) strTxtData += "ą";
                                else if (char1 == 132 && utfWide[i] == 196) strTxtData += "Ą";
                                else if (char1 == 141) strTxtData += "č";
                                else if (char1 == 140)  strTxtData += "Č";
                                else if (char1 == 153) strTxtData += "ę";
                                else if (char1 == 152)  strTxtData += "Ę";
                                else if (char1 == 151) strTxtData += "ė";
                                else if (char1 == 150)  strTxtData += "Ė";
                                else if (char1 == 175) strTxtData += "į";
                                else if (char1 == 174)  strTxtData += "Į";
                                else if (char1 == 161) strTxtData += "š";
                                else if (char1 == 160)  strTxtData += "Š";
                                else if (char1 == 179) strTxtData += "ų";
                                else if (char1 == 178)  strTxtData += "Ų";
                                else if (char1 == 171) strTxtData += "ū";
                                else if (char1 == 170)  strTxtData += "Ū";
                                else if (char1 == 190) strTxtData += "ž";
                                else if (char1 == 189)  strTxtData += "Ž";
                                // LV āēģķļņ | ĀĒĢĶĻŅ
                                //    129 147 171
                                else if (char1 == 129) strTxtData += "ā";
                                else if (char1 == 128)  strTxtData += "Ā";
                                else if (char1 == 147) strTxtData += "ē";
                                else if (char1 == 146)  strTxtData += "Ē";
                                else if (char1 == 163) strTxtData += "ģ";
                                else if (char1 == 162)  strTxtData += "Ģ";
                                else if (char1 == 183) strTxtData += "ķ";
                                else if (char1 == 182)  strTxtData += "Ķ";
                                else if (char1 == 188) strTxtData += "ļ";
                                else if (char1 == 187)  strTxtData += "Ļ";
                                else if (char1 == 134) strTxtData += "ņ";
                                else if (char1 == 133)  strTxtData += "Ņ";

                                else i--;

                                i++;
                                continue;
                            }
                            else if (utfWide[i] == 195)
                            {
                                byte char1 = utfWide[i + 1];

                                // 195: EE  õäöü | ÕÄÖÜ ŠŽ šž
                                //          181 164 182 188
                                if (char1 == 181) strTxtData += "õ";
                                else if (char1 == 149)  strTxtData += "Õ";
                                else if (char1 == 164) strTxtData += "ä";
                                else if (char1 == 132)  strTxtData += "Ä";
                                else if (char1 == 182) strTxtData += "ö";
                                else if (char1 == 150)  strTxtData += "Ö";
                                else if (char1 == 188) strTxtData += "ü";
                                else if (char1 == 156)  strTxtData += "Ü";
                                else i--;

                                i++;
                                continue;
                            }

                            // Because char always saves 2 bytes, fill char with 0
                            byte[] utf8Container = new byte[2] { utfWide[i], 0 };

                            if (utf8Container[0] > 0 || utf8Container[1] > 0)
                                // get NVARCHAR bytes
                                strTxtData += BitConverter.ToChar(utf8Container, 0);

                        }
                    }
                    else
                        strTxtData = Encoding.Default.GetString(dataBytes);

                }

            }

            // before returning close the clipboard
            CloseClipboard();

            return strTxtData;
        }

    }
}
