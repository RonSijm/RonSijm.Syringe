using System.Reflection;

namespace RonSijm.Syringe.Extensions;

internal static class MethodInfoExtensions
{
	public static string GetClassNameAndMethodName(this MethodInfo methodInfo)
	{
		if (methodInfo is null)
			throw new ArgumentNullException(nameof(methodInfo));

		return $"Method \"{methodInfo.Name}\" on class \"{methodInfo.DeclaringType.FullName}\"";
	}
}
