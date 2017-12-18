using FNF.Mask.Fody;
using WeavableAssembly.DTO;

namespace Mono.Cecil.Debugger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var assemblyPath = typeof(Person).Module.FullyQualifiedName;
            var module = ModuleDefinition.ReadModule(assemblyPath);
            var ssnMaskWeaver = new ModuleWeaver { ModuleDefinition = module };
            ssnMaskWeaver.Execute();
        }
    }
}