namespace RonSijm.Syringe.DependencyInjection;

internal static class TypeHelper
{
	internal static Type[] GetGenericParametersForImplementedInterface(Type implementingType, Type genericInterfaceRequired)
	{
		foreach(var interfaceType in implementingType.GetInterfaces())
		{
			if (!interfaceType.IsGenericType)
				continue;

			var genericTypeForInterface = interfaceType.GetGenericTypeDefinition();
			if (genericTypeForInterface == genericInterfaceRequired)
				return interfaceType.GetGenericArguments();
		}
		return null;
	}
}
