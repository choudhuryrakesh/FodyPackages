using Mono.Cecil.Cil;
using SSNMask.Fody.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Cecil = Mono.Cecil;

namespace SSNMask.Fody
{
    public class ModuleWeaver
    {
        private readonly MethodInfo _debugWriteLine;
        private readonly Func<Cecil.PropertyDefinition, bool> _shouldMaskProperties;
        private readonly MethodInfo _mask;

        public Cecil.ModuleDefinition ModuleDefinition { get; set; }

        public ModuleWeaver()
        {
            _debugWriteLine = typeof(Debug).GetMethods()
                .Where(method => method.Name == nameof(Debug.WriteLine))
                .Single(wl =>
                {
                    var parameters = wl.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                });

            _shouldMaskProperties = (p) => p.CustomAttributes
                .Any(a => a.AttributeType.FullName == typeof(MaskSSNAttribute).FullName);

            _mask = typeof(Masker).GetMethods()
                .Where(method => method.Name == nameof(Masker.Mask))
                .Single(m =>
                {
                    var parameters = m.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                });
        }

        public void Execute()
        {
            foreach (var type in ModuleDefinition.Types)
            {
                var propertiesToMask = type.Properties.Where(_shouldMaskProperties).ToList();
                foreach (var property in propertiesToMask)
                {
                    var attributeName = property.CustomAttributes[0].AttributeType.FullName;

                    AddMaskingProperty(type, property);
                    AddShouldSerializeMethod(type, property);
                }
            }
        }

        private void AddShouldSerializeMethod(Cecil.TypeDefinition type, Cecil.PropertyDefinition property)
        {
            var shouldSerializeMethod = new Cecil.MethodDefinition("ShouldSerialize" + property.Name,
                Cecil.MethodAttributes.Public, ModuleDefinition.TypeSystem.Boolean);

            var body = shouldSerializeMethod.Body;
            body.InitLocals = true;
            body.Variables.Add(new VariableDefinition(ModuleDefinition.TypeSystem.Boolean));

            var instructions = body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Nop));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            var ldLoc0 = Instruction.Create(OpCodes.Ldloc_0);
            instructions.Add(Instruction.Create(OpCodes.Br_S, ldLoc0));
            instructions.Add(ldLoc0);
            instructions.Add(Instruction.Create(OpCodes.Ret));

            type.Methods.Add(shouldSerializeMethod);
        }

        private void AddMaskingProperty(Cecil.TypeDefinition type, Cecil.PropertyDefinition property)
        {
            var maskedPropety = new Cecil.PropertyDefinition(property.Name + "Mask", Cecil.PropertyAttributes.None, ModuleDefinition.TypeSystem.String);
            //maskedPropety.GetMethod =
            type.Properties.Add(maskedPropety);
        }

        private void WeaveMaskTo(Cecil.PropertyDefinition property)
        {
            var getBody = property.GetMethod.Body;

            getBody.InitLocals = true;
            getBody.Variables.Add(new VariableDefinition(ModuleDefinition.TypeSystem.String));

            var ilProcessor = getBody.GetILProcessor();
            var returnInstruction = getBody.Instructions.Where(i => i.OpCode == OpCodes.Ret).Single();
            foreach (var instruction in GetMaskInstructions())
            {
                ilProcessor.InsertBefore(returnInstruction, instruction);
            }
        }

        private IEnumerable<Instruction> GetMaskInstructions()
        {
            var loadLoc = Instruction.Create(OpCodes.Ldloc_0);

            yield return Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(_mask));
            yield return Instruction.Create(OpCodes.Stloc_0);
            yield return Instruction.Create(OpCodes.Br_S, loadLoc);
            yield return loadLoc;
        }

        private void WeaveDebugWriteLineTo(Cecil.PropertyDefinition property)
        {
            var getBody = property.GetMethod.Body;
            var ilProcessor = getBody.GetILProcessor();

            var currentInstruction = Instruction.Create(OpCodes.Nop);
            ilProcessor.InsertBefore(getBody.Instructions.First(), currentInstruction);

            foreach (var instruction in GetWriteLineInstructions())
            {
                ilProcessor.InsertAfter(currentInstruction, instruction);
                currentInstruction = instruction;
            }
        }

        private IEnumerable<Instruction> GetWriteLineInstructions()
        {
            yield return Instruction.Create(OpCodes.Ldstr, "Fody weaved this part.");
            yield return Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(_debugWriteLine));
        }
    }
}