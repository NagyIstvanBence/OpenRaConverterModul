using System;
using System.Collections.Generic;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models.CodeStructure;
using OpenRA.Converter.Core.Models.DecisionTree;

namespace OpenRA.Converter.Infrastructure.Services
{
    public class TraitSynthesisService : ITraitSynthesisService
    {
        public CsClass SynthesizeTrait(DecisionNode rootNode, string traitName)
        {
            // 1. Create the Info Class (Configuration)
            var infoClass = new CsClass
            {
                Name = $"{traitName}Info",
                Inherits = "ConditionalTraitInfo",
                Usings = new List<string> { "OpenRA.Traits" }
            };

            // Standard OpenRA boilerplate for Info classes
            infoClass.Fields.Add(new CsField
            {
                Name = "RequiresCondition",
                Type = "string",
                IsExposedToYaml = true,
                Description = "Condition required to enable this trait."
            });

            // 2. Create the Logic Class (Implementation)
            var logicClass = new CsClass
            {
                Name = traitName,
                Inherits = $"ConditionalTrait<{traitName}Info>",
                PairedInfoClass = infoClass
            };

            // 3. Analyze Tree to determine required interfaces
            // If the tree has time-based logic, we need ITick.
            // For now, we default to adding ITick as most logic requires it.
            logicClass.Interfaces.Add("ITick");

            // 4. Generate the Tick method
            var tickMethod = new CsMethod
            {
                Name = "Tick",
                ReturnType = "void",
                ExplicitInterfaceImplementation = "ITick",
                Parameters = new List<CsParameter> { new CsParameter("Actor", "self") }
            };

            // Standard check for disabled trait
            tickMethod.BodyLines.Add("if (IsTraitDisabled) return;");

            // TODO: Recursively walk rootNode to generate the logic body
            // ProcessNode(rootNode, tickMethod.BodyLines);

            logicClass.Methods.Add(tickMethod);

            return logicClass;
        }
    }
}