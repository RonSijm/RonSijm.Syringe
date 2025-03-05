using System.Reflection;
using Fluxor;

namespace RonSijm.Syringe
{
    public static class GetAddReducerMethodHelper
    {
        private const string AddReducerMethodName = nameof(IFeature<object>.AddReducer);

        public static MethodInfo GetAddReducerMethod(Type featureImplementingType)
        {
            var featureAddReducerMethodInfo = featureImplementingType.GetMethod(AddReducerMethodName);
            return featureAddReducerMethodInfo;
        }
    }
}