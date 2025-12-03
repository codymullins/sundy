using AutoFixture.Kernel;

namespace Sundy.Test;

public class ServiceProviderRelay(IServiceProvider serviceProvider) : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type)
        {
            var service = serviceProvider.GetService(type);
            if (service != null)
                return service;
        }

        return new NoSpecimen();
    }
}