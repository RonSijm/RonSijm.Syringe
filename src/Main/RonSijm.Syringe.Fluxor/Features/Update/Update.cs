namespace RonSijm.Syringe;

public class Update<T>(Action<T> update) where T : class, new()
{
    public Action<T> UpdateAction { get; set; } = update;

    public T Handle(object input)
    {
        var result = new T();
        var typedInput = input as T;

        CopyPropertiesHelper.CopyProperties(typeof(T), typedInput, result);
        UpdateAction(result);

        return result;
    }
}