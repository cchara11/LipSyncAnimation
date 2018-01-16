using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

[AttributeUsageAttribute(AttributeTargets.All)]
public class DescriptionWithValueAttribute : DescriptionAttribute
{
    public DescriptionWithValueAttribute(string name) : base(name)
    {
    }

    public DescriptionWithValueAttribute(string description, string value) : base(description)
    {
        this.Value = value;
    }

    public string Value { get; private set; }
}
