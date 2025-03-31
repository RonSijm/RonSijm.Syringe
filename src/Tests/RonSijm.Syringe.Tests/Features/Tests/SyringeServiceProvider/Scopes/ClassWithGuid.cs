using System.Diagnostics;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.Scopes
{
    [DebuggerDisplay("{Id}")]
    public class ClassWithGuid
    {
        public ClassWithGuid()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}
