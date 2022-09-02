using DBInterface;
using System;

namespace DBInterface_XUnit_Tests.ExternalTypes
{
    internal class ExternalLookup: ILookup
    {
        Lookup lu;

        public ExternalLookup() { lu = Lookup.Build("Example"); ExternalData = 2.71; }
        public ExternalLookup(string key) { lu = Lookup.Build(key); ExternalData = 3.14; }

        public double ExternalData { get; set; }

        public virtual string KeyCopy => lu.KeyCopy;
    }

    internal class BadExternalLookup: ExternalLookup
    {
        public static readonly string AnyInstanceReturnsThisForKeyCopy = "Muahaha! I did not copy faithfully!";
        public override string KeyCopy => AnyInstanceReturnsThisForKeyCopy;
    }
}
