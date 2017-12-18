using Mono.Cecil;

namespace FNF.Mask.Fody.Maskers
{
    public class DefaultMasker : Masker
    {
        internal DefaultMasker(ModuleDefinition module) : base(module)
        {
        }

        internal override void Process(PropertyDefinition property, TypeDefinition inClass)
        {
        }
    }
}