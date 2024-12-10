using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe
{
    public partial class SyringeServiceCollection : IServiceCollection
    {
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return InnerServiceCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)InnerServiceCollection).GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            InnerServiceCollection.Add(item);
        }

        public void Clear()
        {
            InnerServiceCollection.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return InnerServiceCollection.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            InnerServiceCollection.CopyTo(array, arrayIndex);
        }

        public bool Remove(ServiceDescriptor item)
        {
            return InnerServiceCollection.Remove(item);
        }

        public int Count => InnerServiceCollection.Count;

        public bool IsReadOnly => InnerServiceCollection.IsReadOnly;

        public int IndexOf(ServiceDescriptor item)
        {
            return InnerServiceCollection.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            InnerServiceCollection.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            InnerServiceCollection.RemoveAt(index);
        }

        public ServiceDescriptor this[int index]
        {
            get => InnerServiceCollection[index];
            set => InnerServiceCollection[index] = value;
        }
    }
}
