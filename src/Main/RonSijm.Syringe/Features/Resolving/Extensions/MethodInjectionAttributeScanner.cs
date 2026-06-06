using System.Reflection;

namespace RonSijm.Syringe;

internal static class MethodInjectionAttributeScanner
{
    public static IEnumerable<(ParameterInfo Parameter, bool IsKeyed, object Key)> GetInjectableParameters(MethodInfo method)
    {
        foreach (var parameter in method.GetParameters())
        {
            var attribute = parameter.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name is "FromServicesAttribute" or "FromKeyedServicesAttribute");

            if (attribute == null)
            {
                continue;
            }

            var isKeyed = attribute.GetType().Name == "FromKeyedServicesAttribute";
            var key = isKeyed ? GetKey(attribute) : null;

            yield return (parameter, isKeyed, key);
        }
    }

    public static string FormatMissingParameter(MethodInfo method, ParameterInfo parameter, object key)
    {
        return key != null
            ? $"Unable to resolve required parameter '{parameter.Name}' of type '{parameter.ParameterType}' with key '{key}' on method '{method.DeclaringType?.FullName}.{method.Name}'."
            : $"Unable to resolve required parameter '{parameter.Name}' of type '{parameter.ParameterType}' on method '{method.DeclaringType?.FullName}.{method.Name}'.";
    }

    private static object GetKey(Attribute attribute)
    {
        var keyProperty = attribute.GetType().GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
        if (keyProperty == null || !keyProperty.CanRead)
        {
            return null;
        }

        var value = keyProperty.GetValue(attribute);
        if (value is string stringValue)
        {
            return string.IsNullOrEmpty(stringValue) ? null : stringValue;
        }

        return value;
    }
}
