using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class
    | AttributeTargets.Method
    | AttributeTargets.Property
    | AttributeTargets.Field, Inherited = false)]
internal class OnlyMainThread : Attribute
{
}

[AttributeUsage(AttributeTargets.Class
    | AttributeTargets.Method
    | AttributeTargets.Property
    | AttributeTargets.Field, Inherited = false)]
internal class OnlyWorkerThread : Attribute
{
}
[AttributeUsage(AttributeTargets.Class
    | AttributeTargets.Method
    | AttributeTargets.Property
    | AttributeTargets.Field, Inherited = false)]
internal class BothThreads : Attribute
{
}
