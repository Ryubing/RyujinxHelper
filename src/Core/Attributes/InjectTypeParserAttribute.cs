using System;

namespace Volte.Core.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InjectTypeParserAttribute(bool overridePrimitive = false) : Attribute
    {
        public bool OverridePrimitive => overridePrimitive;
    }
}
