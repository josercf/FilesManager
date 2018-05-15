using Microsoft.Azure.WebJobs.Description;
using System;

namespace FaispCertifieldProcess.DependencyInjection
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
    }
}
