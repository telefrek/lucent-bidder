using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lucent.Common;

/// <summary>
/// Contains global extension methods that are useful in a variety of scenarios and not scoped to a single
/// package or utility
/// </summary>
public static class GlobalExtensions
{
    /// <summary>
    /// Format a string in place
    /// </summary>
    /// <param name="format">The format string to use</param>
    /// <param name="data">The data to substitute into the format</param>
    /// <returns>A formatted string</returns>
    public static string FormatWith(this string format, params object[] data) => string.Format(format, data);

    /// <summary>
    /// Sets the value in the dictionary, regardless of whether or not it exists
    /// </summary>
    /// <param name="dictionary">The dictionary to modify</param>
    /// <param name="key">The key to search with</param>
    /// <param name="value">The value to upsert</param>
    /// <typeparam name="T">The key type</typeparam>
    /// <typeparam name="F">The value type</typeparam>
    public static void Set<T, F>(this IDictionary<T, F> dictionary, T key, F value)
    {
        // I just hate typing this all the time honestly...
        if (dictionary.ContainsKey(key))
            dictionary[key] = value;
        else
            dictionary.Add(key, value);
    }

    /// <summary>
    /// Creates an instance of the given type, injecting parameters where possible from the provider
    /// 
    /// Throws a LucentException
    /// </summary>
    /// <param name="provider">The provider to use for parameter resolution</param>
    /// <param name="supplied">The supplied parameters</param>
    /// <typeparam name="T">The type of object to craete</typeparam>
    /// <exception cref="Lucent.Common.LucentException">If there is a problem resolving the object</exception>
    /// <returns>A new instance of the object if it can be created</returns>
    public static T CreateInstance<T>(this IServiceProvider provider, params object[] supplied)
    {
        var types = supplied.Select(s => s.GetType()).ToArray();
        var cinfo = typeof(T).GetConstructors().Where(c => IsMatch(c, types)).FirstOrDefault();

        if (cinfo != null)
        {
            try
            {
                // Basically the same loop as match test but with calls to provider to resolve missing parameters
                var pArr = cinfo.GetParameters();
                var pMap = new object[pArr.Length];

                // Fill the supplied and parameter mixed
                var idx = 0;
                var i = 0;
                for (; i < pArr.Length && idx < types.Length; ++i)
                    if (pArr[i].ParameterType.IsAssignableFrom(types[idx]))
                        pMap[i] = supplied[idx++];
                    else
                        pMap[i] = provider.GetService(pArr[i].ParameterType);

                // Finish any remaining injections
                for(; i < pArr.Length; ++i)
                    pMap[i] = provider.GetService(pArr[i].ParameterType);

                // Hope for the best
                return (T)cinfo.Invoke(pMap);
            }
            catch (Exception e)
            {
                // Wrap and throw it
                throw new LucentException("Failed to create object", e);
            }
        }

        // Boom goes the dynamite!
        throw new LucentException("Failed to locate constructor to match parameters supplied with provider");
    }

    /// <summary>
    /// Test to validate if it's possible to use this constructor with the supplied types
    /// 
    /// NOTE: This is really not safe, but a convenient hack for this project
    /// </summary>
    /// <param name="constructorInfo">The constructor to test</param>
    /// <param name="suppliedTypes">The types supplied already</param>
    /// <returns>True if the constructor is a match for creating</returns>
    static bool IsMatch(ConstructorInfo constructorInfo, Type[] suppliedTypes)
    {
        try
        {
            // Filter out non-public and static constructors
            if (!constructorInfo.IsPublic) return false;
            if (constructorInfo.IsStatic) return false;

            // Get the parameters
            var pArr = constructorInfo.GetParameters();

            // Can't use one with less than the supplied
            if (pArr.Length < suppliedTypes.Length) return false;

            // Supplied types must be in order, interfaces ASSUMED to be in service provider if not matched...
            var idx = 0;
            var i = 0;
            for (i = 0; i < pArr.Length && idx < suppliedTypes.Length; ++i)
            {
                if (pArr[i].ParameterType.IsAssignableFrom(suppliedTypes[idx]))
                    idx++;
                else if (!pArr[i].ParameterType.IsInterface)
                    break;
            }

            // May have residual types left over
            for (; i < pArr.Length; ++i)
                if (!pArr[i].ParameterType.IsInterface)
                    break;

            // Have to make it through both arrays for this to be true
            return idx == suppliedTypes.Length && i == pArr.Length;
        }
        catch
        {
            return false;
        }
    }
}