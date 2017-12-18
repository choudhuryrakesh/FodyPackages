using FNF.Mask.Fody.Attributes;
using Mono.Cecil;

namespace FNF.Mask.Fody.Maskers
{
    public abstract class Masker
    {
        protected readonly ModuleDefinition Module;

        protected Masker(ModuleDefinition module)
        {
            Module = module;
        }

        internal abstract void Process(PropertyDefinition property, TypeDefinition inClass);

        internal static Masker For(ModuleDefinition module, MaskAttribute attribute)
        {
            if (attribute is MaskSSNAttribute)
                return new SSNMasker(module);

            return new DefaultMasker(module);
        }
    }
}