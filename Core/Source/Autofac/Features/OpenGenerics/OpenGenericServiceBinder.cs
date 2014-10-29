// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if ASPNETCORE50
using System.Reflection;
#endif
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OpenGenerics
{
    static class OpenGenericServiceBinder
    {
        public static bool TryBindServiceType(
            Service service,
            IEnumerable<Service> configuredOpenGenericServices,
            Type openGenericImplementationType,
            out Type constructedImplementationType,
            out IEnumerable<Service> constructedServices)
        {
            var swt = service as IServiceWithType;
#if !ASPNETCORE50
            if (swt != null && swt.ServiceType.IsGenericType)
#else
            if (swt != null && swt.ServiceType.GetTypeInfo().IsGenericType)
#endif
            {
                var definitionService = (IServiceWithType)swt.ChangeType(swt.ServiceType.GetGenericTypeDefinition());
                var serviceGenericArguments = swt.ServiceType.GetGenericArguments();

                if (configuredOpenGenericServices.Cast<IServiceWithType>().Any(s => s.Equals(definitionService)))
                {
                    var implementorGenericArguments = TryMapImplementationGenericArguments(
                        openGenericImplementationType, swt.ServiceType, definitionService.ServiceType, serviceGenericArguments);

                    if (!implementorGenericArguments.Any(a => a == null) &&
                        openGenericImplementationType.IsCompatibleWithGenericParameterConstraints(implementorGenericArguments))
                    {
                        var constructedImplementationTypeTmp = openGenericImplementationType.MakeGenericType(implementorGenericArguments);

                        // This needs looking at
                        var implementedServices = (from IServiceWithType s in configuredOpenGenericServices
                                                   let genericService = s.ServiceType.MakeGenericType(serviceGenericArguments)
                                                   where genericService.IsAssignableFrom(constructedImplementationTypeTmp)
                                                   select s.ChangeType(genericService)).ToArray();

                        if (implementedServices.Length > 0)
                        {
                            constructedImplementationType = constructedImplementationTypeTmp;
                            constructedServices = implementedServices;
                            return true;
                        }
                    }
                }
            }

            constructedImplementationType = null;
            constructedServices = null;
            return false;
        }

        static Type[] TryMapImplementationGenericArguments(Type implementationType, Type serviceType, Type serviceTypeDefinition, Type[] serviceGenericArguments)
        {
            if (serviceTypeDefinition == implementationType)
                return serviceGenericArguments;

            var implementationGenericArgumentDefinitions = implementationType.GetGenericArguments();
#if !ASPNETCORE50
            var serviceArgumentDefinitions = serviceType.IsInterface ?
#else
            var serviceArgumentDefinitions = serviceType.GetTypeInfo().IsInterface ?
#endif
                    GetInterface(implementationType, serviceType).GetGenericArguments() :
                    serviceTypeDefinition.GetGenericArguments();

            var serviceArgumentDefinitionToArgumentMapping = serviceArgumentDefinitions.Zip(serviceGenericArguments, (a, b) => new KeyValuePair<Type, Type>(a, b));

            return implementationGenericArgumentDefinitions
                .Select(implementationGenericArgumentDefinition => TryFindServiceArgumentForImplementationArgumentDefinition(
                    implementationGenericArgumentDefinition, serviceArgumentDefinitionToArgumentMapping))
                .ToArray();
        }

        static Type GetInterface(Type implementationType, Type serviceType)
        {
            try
            {
                return implementationType.GetInterfaces()
                    .Single(i => i.Name == serviceType.Name && i.Namespace == serviceType.Namespace);
            }
            catch (InvalidOperationException)
            {
                var message = string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorDoesntImplementService, implementationType.FullName, serviceType.FullName);
                throw new InvalidOperationException(message);
            }
        }

        static Type TryFindServiceArgumentForImplementationArgumentDefinition(Type implementationGenericArgumentDefinition, IEnumerable<KeyValuePair<Type, Type>> serviceArgumentDefinitionToArgument)
        {
            var matchingRegularType = serviceArgumentDefinitionToArgument
#if !ASPNETCORE50
                .Where(argdef => !argdef.Key.IsGenericType && implementationGenericArgumentDefinition.Name == argdef.Key.Name)
#else
                .Where(argdef => !argdef.Key.GetTypeInfo().IsGenericType && implementationGenericArgumentDefinition.Name == argdef.Key.Name)
#endif
                .Select(argdef => argdef.Value)
                .FirstOrDefault();

            if (matchingRegularType != null)
                return matchingRegularType;

            return serviceArgumentDefinitionToArgument
#if !ASPNETCORE50
                .Where(argdef => argdef.Key.IsGenericType && argdef.Value.GetGenericArguments().Length > 0)
#else
                .Where(argdef => argdef.Key.GetTypeInfo().IsGenericType && argdef.Value.GetGenericArguments().Length > 0)
#endif
                .Select(argdef => TryFindServiceArgumentForImplementationArgumentDefinition(
                    implementationGenericArgumentDefinition, argdef.Key.GetGenericArguments().Zip(argdef.Value.GetGenericArguments(), (a, b) => new KeyValuePair<Type, Type>(a, b))))
                .FirstOrDefault();
        }

        public static void EnforceBindable(Type implementationType, IEnumerable<Service> services)
        {
            if (implementationType == null) throw new ArgumentNullException("implementationType");
            if (services == null) throw new ArgumentNullException("services");

#if !ASPNETCORE50
            if (!implementationType.IsGenericTypeDefinition)
#else
            if (!implementationType.GetTypeInfo().IsGenericTypeDefinition)
#endif
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorMustBeOpenGenericTypeDefinition, implementationType));

            foreach (IServiceWithType service in services)
            {
#if !ASPNETCORE50
                if (!service.ServiceType.IsGenericTypeDefinition)
#else
                if (!service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
#endif
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ServiceTypeMustBeOpenGenericTypeDefinition, service));

#if !ASPNETCORE50
                if (service.ServiceType.IsInterface)
#else
                if (service.ServiceType.GetTypeInfo().IsInterface)
#endif
                {
                    if (GetInterface(implementationType, service.ServiceType) == null)
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.InterfaceIsNotImplemented, implementationType, service));
                }
                else
                {
#if !ASPNETCORE50
                    if (!Traverse.Across(implementationType, t => t.BaseType).Any(t => IsCompatibleGenericClassDefinition(t, service.ServiceType)))
#else
                    if (!Traverse.Across(implementationType, t => t.GetTypeInfo().BaseType).Any(t => IsCompatibleGenericClassDefinition(t, service.ServiceType)))
#endif
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.TypesAreNotConvertible, implementationType, service));
                }
            }
        }

        static bool IsCompatibleGenericClassDefinition(Type implementor, Type serviceType)
        {
#if !ASPNETCORE50
            return implementor == serviceType || implementor.IsGenericType && implementor.GetGenericTypeDefinition() == serviceType;
#else
            return implementor == serviceType || implementor.GetTypeInfo().IsGenericType && implementor.GetGenericTypeDefinition() == serviceType;
#endif
        }
    }
}
