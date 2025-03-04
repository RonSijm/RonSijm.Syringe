namespace RonSijm.Syringe
{
    public abstract class AdditionProvider
    {
        protected Func<Type, bool> Criteria { get; set; }

        public virtual bool IsMatch(Type serviceType)
        {
            return Criteria(serviceType);
        }

        public abstract object Create(Type serviceType, SyringeServiceProvider serviceProvider);
    }
}