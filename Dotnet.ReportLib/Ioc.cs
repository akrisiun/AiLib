using System;
using System.Collections.Generic;
using System.Reflection;

// http://kenegozi.com/blog/2008/01/17/its-my-turn-to-build-an-ioc-container-in-15-minutes-and-33-lines
// .all me a sloppy-coder, call me whadever-ya-like. It just works.

namespace Dotnet.Dependancy
{
    static class IoC
    {
        static readonly IDictionary<Type, Type> types = new Dictionary<Type, Type>();

        public static void Register<TContract, TImplementation>()
        {
            types[typeof(TContract)] = typeof(TImplementation);
        }

        public static T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public static object Resolve(Type contract)
        {
            Type implementation = types[contract];
            ConstructorInfo constructor = implementation.GetConstructors()[0];
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 1)
            {
                return Activator.CreateInstance(implementation);
            }

            List<object> parameters = new List<object>(constructorParameters.Length);
            foreach (ParameterInfo parameterInfo in constructorParameters)
            {
                parameters.Add(Resolve(parameterInfo.ParameterType));
            }

            return constructor.Invoke(parameters.ToArray());
        }
    }

    // Ok, I�ve cheated you can do:
    // IoC.Register<IBuildDirectoryStructureService, BuildDirectoryStructureService>(); 
    // IBuildDirectoryStructureService service = IoC.Resolve<IBuildDirectoryStructureService>();
}