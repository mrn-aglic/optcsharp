using System;

namespace CSharpOptBackend.Variables
{
    public class Var
    {
        public string Name { get; }
        public object Value { get; }
        public Type Type { get; }
        public string TypeName { get; }

        public Var(string name, object value)
        {
            Name = name;
            Value = value;

            Type = value?.GetType();
            TypeName = Type?.Name;
        }

        public Var(string name, string type)
        {
            Name = name;
            Type = Type.GetType(type);
            TypeName = type;
        }
    }
}