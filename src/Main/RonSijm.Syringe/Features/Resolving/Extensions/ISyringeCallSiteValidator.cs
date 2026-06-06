using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

public interface ISyringeCallSiteValidator
{
    void ValidateCallSite(ServiceCallSite callSite);
}
