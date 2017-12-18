using System;
using System.Text.RegularExpressions;

namespace SSNMask.Fody.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class MaskBaseAttribute : Attribute
    {
    }


    public class MaskSSNAttribute:MaskBaseAttribute
    {

    }
}