namespace RonSijm.Syringe
{
    public class SingletonProvider(object instance) : AdditionProvider
    {
        private readonly Type _instanceType = instance.GetType();

        public override bool IsMatch(Type serviceType)
        {
            return _instanceType == serviceType;
        }

        public override object Create(Type serviceType, SyringeServiceProvider serviceProvider)
        {
            return instance;
        }
    }
}