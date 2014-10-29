﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Autofac.Core.Lifetime {
    using System;
#if ASPNETCORE50
    using System.Reflection;
#endif


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class MatchingScopeLifetimeResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MatchingScopeLifetimeResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
#if !ASPNETCORE50
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Autofac.Core.Lifetime.MatchingScopeLifetimeResources", typeof(MatchingScopeLifetimeResources).Assembly);
#else
                    //TODO: Removed namespace to work around https://github.com/aspnet/KRuntime/issues/738
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Autofac.MatchingScopeLifetimeResources", typeof(MatchingScopeLifetimeResources).GetTypeInfo().Assembly);
#endif
                        resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No scope with a Tag matching &apos;{0}&apos; is visible from the scope in which the instance was requested. This generally indicates that a component registered as per-HTTP request is being requested by a SingleInstance() component (or a similar scenario.) Under the web integration always request dependencies from the DependencyResolver.Current or ILifetimeScopeProvider.RequestLifetime, never from the container itself..
        /// </summary>
        internal static string MatchingScopeNotFound {
            get {
                return ResourceManager.GetString("MatchingScopeNotFound", resourceCulture);
            }
        }
    }
}
