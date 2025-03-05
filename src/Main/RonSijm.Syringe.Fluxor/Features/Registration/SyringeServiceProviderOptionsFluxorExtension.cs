namespace RonSijm.Syringe
{
    public static class SyringeServiceProviderOptionsFluxorExtension
    {
        public static void UseFluxor(this SyringeServiceProviderOptions providerOptions)
        {
            providerOptions.AfterBuildExtensions.Add(new WireFluxorAfterBuildExtension());
        }
    }
}