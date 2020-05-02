using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dotnet.Reflection
{
    public static class StreamConvert
    {
        public static void CopyStream(this StreamReader source, Stream destination, byte[] buffer = null)
        {
            source.BaseStream.CopyStream(destination, buffer);
        }

        private static object lockBuff = new object();
        private static byte[] Buffer65K = new byte[0xFFFF];

        public static void CopyStream(this Stream source, Stream destination, byte[] buffer = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
#if DEBUG
            // Ensure a reasonable size of buffer is used without being prohibitive.
            if (buffer != null && buffer.Length < 128)
            {
                throw new ArgumentException("Buffer is too small", "buffer");
            }
#endif
            bool copying = true;

            if (buffer == null)
            {
                buffer = Buffer65K;
                lock (lockBuff)
                { 
                        while (copying)
                    {
                        int bytesRead = source.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            destination.Write(buffer, 0, bytesRead);
                        }
                        else
                        {
                            destination.Flush();
                            copying = false;
                        }
                    }
                }
                return;
            }

            while (copying)
            {
                int bytesRead = source.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    destination.Write(buffer, 0, bytesRead);
                }
                else
                {
                    destination.Flush();
                    copying = false;
                }
            }
        }
    }
}
