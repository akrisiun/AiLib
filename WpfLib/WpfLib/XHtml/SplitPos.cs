using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.XHtml
{
    public class SplitPos
    {
        public SplitPos(string text = "")
        {
            Text = text;
            SplitPositions = null;
            SplitText = null;
        }

        public string Text { get; private set; }
        public int Length { get { return Text.Length; } }

        public Int32[] SplitPositions { get; private set; }
        public string[] SplitText { get; private set; }

        public IEnumerable<string> SplitWithBegin
        {
            get
            {
                if (SplitPositions.Length > 0 && SplitPositions[0] > 0)
                    yield return Text.Substring(0, SplitPositions[0]);

                if (SplitPositions.Length > 0)
                    for (int i = 0; i < SplitText.Length; i++)
                    {
                        if (i < SplitPositions.Length - 1)
                            yield return Text.Substring(SplitPositions[i], SplitPositions[i + 1] - SplitPositions[i]);
                        else if (i < SplitPositions.Length)
                            yield return Text.Substring(SplitPositions[i]);
                    }

                if (SplitPositions.Length > 0
                    && (SplitPositions.Length == SplitText.Length + 1))
                    yield return Text.Substring(SplitPositions[SplitPositions.Length - 1]);
            }
        }

        public IEnumerable<object[]> SplitWithBeginPos
        {
            get
            {
                for (int i = 0; i < SplitText.Length; i++)
                {
                    if (i < SplitPositions.Length - 1)
                        yield return new object[] 
                                { Text.Substring(SplitPositions[i], SplitPositions[i + 1] - SplitPositions[i]),
                                  i  
                                };
                    else
                        yield return new object[] 
                                { Text.Substring(SplitPositions[i]),
                                  i 
                                };
                }
                if (SplitPositions.Length == SplitText.Length + 1)
                    yield return new object[]
                                { Text.Substring(SplitPositions[SplitPositions.Length - 1]),
                                  SplitPositions.Length - 1 
                                };

            }
        }

        public static SplitPos Split(string text, char[] seperator)
        {
            SplitPos s = new SplitPos(text);

            s.SplitPositions = SplitPosStatic.Array(text, seperator);
            s.SplitText = SplitPosStatic.StrArray;
            return s;
        }

    }

    public static class SplitPosStatic
    {
        // http://referencesource.microsoft.com/#mscorlib/system/string.cs,106
        private static Int32 Length;
        private static string Text;
        private static Int32[] sepList;
        private static string[] strArray;

        public static string[] StrArray { get { return strArray; } }

        static SplitPosStatic()
        {
            sepList = null;
            strArray = null;
            Text = String.Empty;
            Length = 0;
        }

        public static Int32[] Array(string text, char[] splitters)
        {
            Text = text;
            Length = text.Length;

            sepList = null;
            strArray = null;
            if (String.IsNullOrWhiteSpace(Text) || splitters == null || splitters.Length == 0)
                return sepList;

            strArray = SplitInternal(splitters, Length, StringSplitOptions.None);

            return sepList;
        }

        internal static String[] SplitInternal(char[] separator, int count = Int32.MaxValue, StringSplitOptions options = StringSplitOptions.None)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NegativeCount");
            if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
                throw new ArgumentException("Arg_EnumIllegalVal options");
            Contract.Ensures(Contract.Result<String[]>() != null);

            sepList = new int[Length];
            int numReplaces = MakeSeparatorList(separator, ref sepList);

            //Handle the special case of no replaces and special count.
            if (0 == numReplaces || count == 1)
            {
                String[] stringArray = new String[1];
                stringArray[0] = Text;
                sepList = new int[1] { 0 };
                return stringArray;
            }

            return InternalSplitKeepEmptyEntries(null, numReplaces, count);
        }

        private static String[] InternalSplitKeepEmptyEntries(Int32[] lengthList, Int32 numReplaces, int count)
        {
            Contract.Requires(numReplaces >= 0);
            Contract.Requires(count >= 2);

            count--;
            int numActualReplaces = (numReplaces < count) ? numReplaces : count;

            //Allocate space for the new array.
            //+1 for the string from the end of the last replace to the end of the String.
            int arrIndex = 0;
            int currIndex = 0;
            String[] splitStrings = new String[numActualReplaces + 1];

            if (sepList[0] == 0)
            {
                currIndex = sepList[1];
                splitStrings[arrIndex++] = Substring(1, currIndex - 1);
            }

            for (int i = arrIndex; i < numActualReplaces && currIndex < Length; i++)
            {
                splitStrings[arrIndex++] = Substring(currIndex, sepList[i] - currIndex);
                currIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
            }

            //Handle the last string at the end of the array if there is one.
            if (currIndex < Length && numActualReplaces >= 0)
            {
                splitStrings[arrIndex] = Substring(currIndex);
            }
            else if (arrIndex == numActualReplaces)
            {
                //We had a separator character at the end of a string.  Rather than just allowing
                //a null character, we'll replace the last element in the array with an empty string.
                System.Array.Resize<string>(ref splitStrings, arrIndex - 1);
            }

            return splitStrings;
        }


        // [System.Security.SecuritySafeCritical]  // auto-generated
        private static // unsafe 
            int MakeSeparatorList(char[] separator, ref int[] sepList)
        {
            int foundCount = 0;
            int sepListCount = sepList.Length;
            int sepCount = separator.Length;

            // If they passed in a string of chars, actually look for those chars.
            // fixed (char* pwzChars = &m_firstChar, pSepChars = separator)
            {
                for (int i = 0; i < Length && foundCount < sepListCount; i++)
                {
                    // char* pSep = pSepChars;
                    for (int j = 0; j < sepCount; j++) // , pSep++)
                    {
                        // if (pwzChars[i] == *pSep)
                        if (Text[i] == separator[j])
                        {
                            sepList[foundCount++] = i;
                            break;
                        }
                    }
                }
            }

            // static void Resize<T>(ref T[] array, int newSize);
            if (foundCount > 0)
                System.Array.Resize<Int32>(ref sepList, foundCount);
            else
                sepList = null;

            return foundCount;
        }

        // Determines whether two strings match.
        // [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static bool EqualsText(Object obj)
        {
            if (Text == null)                        //this is necessary to guard against reverse-pinvokes and
                throw new NullReferenceException();  //other callers who do not use the callvirt instruction

            String str = obj as String;
            if (str == null)
                return false;

            if (Object.ReferenceEquals(Text, obj))
                return true;

            if (Length != str.Length)
                return false;

            return String.Equals(Text, str);
        }


        private static string Substring(int start, int length = 0)
        {
            return length == 0 ? Text.Substring(start) : Text.Substring(start, length);
        }

        public static String Join<T>(String separator, IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            if (separator == null)
                separator = String.Empty;

            using (IEnumerator<T> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return String.Empty;

                StringBuilder result = StringBuilderCache.Acquire();
                if (en.Current != null)
                {
                    // handle the case that the enumeration has null entries
                    // and the case where their ToString() override is broken
                    string value = en.Current.ToString();
                    if (value != null)
                        result.Append(value);
                }

                while (en.MoveNext())
                {
                    result.Append(separator);
                    if (en.Current != null)
                    {
                        // handle the case that the enumeration has null entries
                        // and the case where their ToString() override is broken
                        string value = en.Current.ToString();
                        if (value != null)
                            result.Append(value);
                    }
                }
                return StringBuilderCache.GetStringAndRelease(result);
            }
        }

        internal static class StringBuilderCache
        {

            private const int MAX_BUILDER_SIZE = 360;
            private static StringBuilder CachedInstance;
            private const int DefaultCapacity = 16;

            public static StringBuilder Acquire(int capacity = DefaultCapacity)
            {
                if (capacity <= MAX_BUILDER_SIZE)
                {
                    StringBuilder sb = StringBuilderCache.CachedInstance;
                    if (sb != null)
                    {
                        // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                        // when the requested size is larger than the current capacity
                        if (capacity <= sb.Capacity)
                        {
                            StringBuilderCache.CachedInstance = null;
                            sb.Clear();
                            return sb;
                        }
                    }
                }
                return new StringBuilder(capacity);
            }

            public static void Release(StringBuilder sb)
            {
                if (sb.Capacity <= MAX_BUILDER_SIZE)
                {
                    StringBuilderCache.CachedInstance = sb;
                }
            }

            public static string GetStringAndRelease(StringBuilder sb)
            {
                string result = sb.ToString();
                Release(sb);
                return result;
            }
        }

        /* mscorlib: ndp\clr\src\bcl\mscorlib.csproj
        [System.Security.SecurityCritical]  // auto-generated
        unsafe string InternalSubString(int startIndex, int length)
        {
            String result = FastAllocateString(length);

            fixed (char* dest = &result.m_firstChar)
            fixed (char* src = &this.m_firstChar)
            {
                wstrcpy(dest, src + startIndex, length);
            }

            return result;
        } */

    }
}
