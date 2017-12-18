using FNF.Mask.Fody.Attributes;
using FNF.Mask.Fody.Maskers;
using Mono.Cecil;
using System;
using System.Linq;

namespace FNF.Mask.Fody
{
    public class ModuleWeaver
    {
        private readonly Func<PropertyDefinition, bool> _isAnyMaskAttributes;
        private readonly Func<CustomAttribute, bool> _isMaskerAttribute;

        public ModuleDefinition ModuleDefinition { get; set; }

        public ModuleWeaver()
        {
            _isMaskerAttribute = a => a.AttributeType.Namespace == typeof(MaskAttribute).Namespace;
            _isAnyMaskAttributes = (p) => p.CustomAttributes.Any(_isMaskerAttribute);
        }

        public void Execute()
        {
            foreach (var @class in ModuleDefinition.Types.Where(t=>t.IsClass))
            {
                var maskableProperties = @class.Properties.Where(_isAnyMaskAttributes).ToList();
                foreach (var property in maskableProperties)
                {
                    var maskAttributes = property.CustomAttributes.Where(_isMaskerAttribute);
                    foreach (var customAttribute in maskAttributes)
                    {
                        var attribute = GetMaskAttribute(customAttribute);
                        var masker = Masker.For(ModuleDefinition, attribute);
                        masker.Process(property: property, inClass: @class);
                    }
                }
            }
        }

        private static MaskAttribute GetMaskAttribute(CustomAttribute customAttribute)
        {
            var attributeName = customAttribute.AttributeType.FullName;
            return Activator.CreateInstance(Type.GetType(attributeName)) as MaskAttribute;
        }
    }
}