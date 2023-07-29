// This is class is a substitute for legacy frameworks
// https://stackoverflow.com/a/70034587/3459458

#if NET462
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter)]
sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
	public string ParameterName { get; } = parameterName;
}
#endif