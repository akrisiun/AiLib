﻿using System;

namespace Ai
{
    /// <summary>
    /// Critical argument check class, throw exception if error
    /// </summary>
    public static class Guard
    {
        public static void Check(bool? isTrue, params object[] param)
        {
            if (!isTrue.HasValue || !isTrue.Value)
            {
                throw new ArgumentException(String.Concat(param) ?? "arguments error");
            }
        }

        public static void CheckWithEx(bool? isTrue, Action<Exception> onError = null, params object[] param)
        { 
            if (!isTrue.HasValue || isTrue.Value)
                return;
            Exception ex = new ArgumentException(String.Concat(param) ?? "arguments error");
            if (onError != null)
                onError(ex);
            else
                throw ex;
        }
        

        public static void CheckArgumentNull(object isNull, params object[] param)
        {
            if (isNull == null)
            {
                throw new ArgumentNullException(String.Concat(param) ?? " argument null error");
            }
        }


        public static ILog Log {get; set;}  // not null

        static Guard ()
	    {
            Log = new GuardLog();
	    }

        public class GuardLog : ILog 
        {
            public void Write(string message)
            {
#if DEBUG
                System.Diagnostics.Trace.Write(message);
#endif
            }

            public void WriteException(string message, Exception ex)
            {
                Write(message + Environment.NewLine + ex.Message);
            }
        }
    }

    // Simple log interface
    public interface ILog
    {
        void Write(string message);
        void WriteException(string message, Exception ex);
    }

}
