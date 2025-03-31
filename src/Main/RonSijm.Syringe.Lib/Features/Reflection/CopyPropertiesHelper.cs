using System.Reflection;

namespace RonSijm.Syringe;

public static class CopyPropertiesHelper
{
    public static T CopyProperties<T>(this T state, T newState = default) where T : new()
    {
        newState ??= new T();
        CopyProperties(typeof(T), state, newState);

        return newState;
    }

    public static void CopyProperties(Type type, object state, object newState)
    {
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanRead && property.CanWrite)
            {
                property.SetValue(newState, property.GetValue(state));
            }
        }
    }

    public static bool CompareObject(Type type, object state, object newState)
    {
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanRead)
            {
                var oldValue = property.GetValue(state);
                var newValue = property.GetValue(newState);

                if (oldValue != newValue)
                {
                    return false;
                }
            }
        }

        return true;
    }
}