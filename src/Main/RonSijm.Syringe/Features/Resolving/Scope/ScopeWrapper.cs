namespace RonSijm.Syringe.Scope;
public class ScopeWrapper(SyringeServiceProvider serviceProvider)
{
    public SyringeServiceProvider ServiceProvider { get; set; } = serviceProvider;
}
