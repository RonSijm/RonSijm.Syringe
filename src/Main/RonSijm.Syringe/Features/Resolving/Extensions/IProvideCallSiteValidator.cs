namespace RonSijm.Syringe;

public interface IProvideCallSiteValidator
{
    ISyringeCallSiteValidator CreateValidator(MicrosoftServiceProvider serviceProvider);
}
