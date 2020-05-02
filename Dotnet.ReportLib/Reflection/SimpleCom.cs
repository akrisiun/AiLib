using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Dotnet.Reflection
{
    public class SimpleCom<T> : SimpleCom where T : class
    {
        public T Object { get { return UnderlyingObject as T; } }
        public X Get<X>(string propertyName)
        {
            X value = default(X);

            try
            {
                value = (X)InstanceType.InvokeMember(propertyName, System.Reflection.BindingFlags.GetProperty, null, UnderlyingObject, null);
            }
            catch (Exception ex) { LastError = ex; }
            // UnderlyingObject.
            return value;
        }

        public X Invoke<X>(string methodName, object[] args = null)
        {
            object result = Invoke(methodName, args);
            return (X)result;
        }

        public object Invoke(string methodName, object[] args = null)
        {
            object result = null;
            try
            {
                result = InvokeMember(InstanceType, UnderlyingObject, methodName, args);
            }
            catch (Exception ex) { LastError = ex; }

            return result;
        }

        public SimpleCom(string progId)
            : base(progId)
        {
        }

        public int Size { get { return System.Runtime.InteropServices.Marshal.SizeOf(UnderlyingObject); } }
    }

    public class SimpleCom : IDisposable
    {
        public static Dictionary<string, Type> TypeCache { get; private set; }

        static SimpleCom()
        {
            TypeCache = new Dictionary<string, Type>();
        }

        public Type InstanceType { get; protected set; }
        public object UnderlyingObject { get; protected set; }
        public Exception LastError { get; set; }

        public void Dispose()
        {
            if (UnderlyingObject == null)
                return;
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(UnderlyingObject);
            UnderlyingObject = null;
        }

        public SimpleCom(string progId)
        {
            InstanceType = (TypeCache.ContainsKey(progId) ? TypeCache[progId] : null)
                        ?? System.Type.GetTypeFromProgID(progId);
            if (null == InstanceType)
            {
                LastError = new ArgumentException("progId not found. " + progId);
            }
            else
            {
                if (!TypeCache.ContainsKey(progId))
                    TypeCache.Add(progId, InstanceType);

                UnderlyingObject = Activator.CreateInstance(InstanceType);
            }
        }

        public static object InvokeMember(Type InstanceType, object UnderlyingObject,
            string methodName, object[] args = null,
            BindingFlags flags = System.Reflection.BindingFlags.InvokeMethod)
        {
            return InstanceType.InvokeMember(methodName, flags, null, UnderlyingObject, args);
        }

    }
}
