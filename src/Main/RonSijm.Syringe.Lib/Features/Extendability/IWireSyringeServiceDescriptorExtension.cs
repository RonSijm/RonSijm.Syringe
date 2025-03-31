namespace RonSijm.Syringe.Extendability;

public interface IWireSyringeServiceDescriptorExtension
{
    public void BeforeBuildServiceProvider(SyringeServiceDescriptor descriptor);
}