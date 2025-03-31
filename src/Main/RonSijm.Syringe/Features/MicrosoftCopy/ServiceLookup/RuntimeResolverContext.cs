namespace RonSijm.Syringe.ServiceLookup;

internal struct RuntimeResolverContext
{
    public ServiceProviderEngineScope Scope { get; set; }

    public RuntimeResolverLock AcquiredLocks { get; set; }
}