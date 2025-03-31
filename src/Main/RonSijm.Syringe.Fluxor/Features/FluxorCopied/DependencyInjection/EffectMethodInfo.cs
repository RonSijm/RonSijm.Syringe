using System.Reflection;
using Fluxor;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection;

internal class EffectMethodInfo
{
	public readonly Type HostClassType;
	public readonly MethodInfo MethodInfo;
	public readonly Type ActionType;
	public readonly bool RequiresActionParameterInMethod;

	public EffectMethodInfo(
		Type hostClassType,
		EffectMethodAttribute attribute,
		MethodInfo methodInfo)
	{
		var methodParameters = methodInfo.GetParameters();
		if (attribute.ActionType is null && methodParameters.Length != 2)
			throw new ArgumentException(
				$"Method must have 2 parameters (action, IDispatcher)"
					+ $" when [{nameof(EffectMethodAttribute)}] has no {nameof(EffectMethodAttribute.ActionType)} specified. "
					+ methodInfo.GetClassNameAndMethodName(),
				nameof(MethodInfo));

		if (attribute.ActionType is not null && methodParameters.Length != 1)
			throw new ArgumentException(
				$"Method must have 1 parameter (IDispatcher)"
					+ $" when [{nameof(EffectMethodAttribute)}] has an {nameof(EffectMethodAttribute.ActionType)} specified. "
					+ methodInfo.GetClassNameAndMethodName(),
				nameof(methodInfo));

		var lastParameterType = methodParameters[methodParameters.Length - 1].ParameterType;
		if (lastParameterType != typeof(IDispatcher))
			throw new ArgumentException(
				$"The last parameter of a method should be an {nameof(IDispatcher)}"
					+ $" when decorated with an [{nameof(EffectMethodAttribute)}]. "
					+ methodInfo.GetClassNameAndMethodName(),
				nameof(methodInfo));

		if (methodInfo.ReturnType != typeof(Task))
			throw new ArgumentException(
				$"Effect methods must have a return type of {nameof(Task)}. " + methodInfo.GetClassNameAndMethodName(),
				nameof(methodInfo));

		HostClassType = hostClassType;
		MethodInfo = methodInfo;
		ActionType = attribute.ActionType ?? methodParameters[0].ParameterType;
		RequiresActionParameterInMethod = attribute.ActionType is null;
	}
}
