namespace RonSijm.Syringe;

public interface IWireSyringeServiceDescriptorExtension
{
    public void BeforeBuildServiceProvider(SyringeServiceDescriptor descriptor);
}