using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace FNF.Mask.Fody.Maskers
{
    public class SSNMasker : Masker
    {
        internal SSNMasker(ModuleDefinition module) : base(module)
        {
        }

        internal override void Process(PropertyDefinition property, TypeDefinition inClass)
        {
            var maskPropertyName = $"{property.Name}Mask";
            var getMethodForMaskProperty = GetMaskPropertyGetMethod(property, maskPropertyName);
            inClass.Methods.Add(getMethodForMaskProperty);

            var maskProperty = new PropertyDefinition(maskPropertyName, PropertyAttributes.None, Module.TypeSystem.String)
            {
                HasThis = true,
                GetMethod = getMethodForMaskProperty,
                IsSpecialName = true,
            };
            inClass.Properties.Add(maskProperty);

            var dataMemberAttribute = GetDataMemberAttribute(property.Name);
            maskProperty.CustomAttributes.Add(dataMemberAttribute);

            var shouldSerialize = GetShouldSerializeMethodFor(property);
            inClass.Methods.Add(shouldSerialize);
        }

        private CustomAttribute GetDataMemberAttribute(string alias)
        {
            var attributeCtor = Module.ImportReference(typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes));
            var customAttribute = new CustomAttribute(attributeCtor);

            var aliasArgument = new CustomAttributeArgument(Module.TypeSystem.String, alias);
            customAttribute.Properties.Add(new CustomAttributeNamedArgument(nameof(DataMemberAttribute.Name), aliasArgument));

            return customAttribute;
        }

        public static string Mask(string field)
        {
            return Regex.Replace(field, @"[\d-]", "#");
        }

        private MethodDefinition GetShouldSerializeMethodFor(PropertyDefinition property)
        {
            var methodName = "ShouldSerialize" + property.Name;
            var shouldSerializeMethod = new MethodDefinition(methodName, MethodAttributes.Public, Module.TypeSystem.Boolean);

            var body = shouldSerializeMethod.Body;
            body.InitLocals = true;
            body.Variables.Add(new VariableDefinition(Module.TypeSystem.Boolean));

            var instructions = body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Nop));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            var ldLoc0 = Instruction.Create(OpCodes.Ldloc_0);
            instructions.Add(Instruction.Create(OpCodes.Br_S, ldLoc0));
            instructions.Add(ldLoc0);
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return shouldSerializeMethod;
        }

        private MethodDefinition GetMaskPropertyGetMethod(PropertyDefinition property, string newName)
        {
            var getMethod = new MethodDefinition($"get_{newName}", MethodAttributes.Public, Module.TypeSystem.String);

            var body = getMethod.Body;
            body.InitLocals = true;
            body.Variables.Add(new VariableDefinition(Module.TypeSystem.String));

            var instructions = body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Nop));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));

            var actualProperty = property.GetMethod;
            instructions.Add(Instruction.Create(OpCodes.Call, actualProperty));

            var masker = typeof(SSNMasker).GetMethods()
             .Where(method => method.Name == nameof(SSNMasker.Mask))
             .Single(m =>
             {
                 var parameters = m.GetParameters();
                 return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
             });
            instructions.Add(Instruction.Create(OpCodes.Call, Module.ImportReference(masker)));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));

            var ldLoc0 = Instruction.Create(OpCodes.Ldloc_0);
            instructions.Add(Instruction.Create(OpCodes.Br_S, ldLoc0));
            instructions.Add(ldLoc0);
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return getMethod;
        }

        #region old RnD methods

        private void WeaveMaskTo(PropertyDefinition property)
        {
            var getBody = property.GetMethod.Body;

            getBody.InitLocals = true;
            getBody.Variables.Add(new VariableDefinition(Module.TypeSystem.String));

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
            var mask = typeof(SSNMasker).GetMethods()
              .Where(method => method.Name == nameof(SSNMasker.Mask))
              .Single(m =>
              {
                  var parameters = m.GetParameters();
                  return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
              });
            yield return Instruction.Create(OpCodes.Call, Module.ImportReference(mask));
            yield return Instruction.Create(OpCodes.Stloc_0);
            yield return Instruction.Create(OpCodes.Br_S, loadLoc);
            yield return loadLoc;
        }

        private void WeaveDebugWriteLineTo(PropertyDefinition property)
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
            var debugWriteLine = typeof(Debug).GetMethods()
           .Where(method => method.Name == nameof(Debug.WriteLine))
           .Single(wl =>
           {
               var parameters = wl.GetParameters();
               return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
           });
            yield return Instruction.Create(OpCodes.Call, Module.ImportReference(debugWriteLine));
        }

        #endregion old RnD methods
    }
}