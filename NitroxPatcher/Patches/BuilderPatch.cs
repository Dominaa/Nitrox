﻿using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class BuilderPatch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryPlace");

        public static readonly OpCode PLACE_BASE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object PLACE_BASE_INJECTION_OPERAND = typeof(BaseGhost).GetMethod("Place");

        public static readonly OpCode PLACE_FURNITURE_INJECTION_OPCODE = OpCodes.Call;
        public static readonly object PLACE_FURNITURE_INJECTION_OPERAND = typeof(SkyEnvironmentChanged).GetMethod("Send", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject), typeof(Component) }, null);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(PLACE_BASE_INJECTION_OPERAND);
            Validate.NotNull(PLACE_FURNITURE_INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(PLACE_BASE_INJECTION_OPCODE) && instruction.operand.Equals(PLACE_BASE_INJECTION_OPERAND))
                {
                    /*
                     *  Multiplayer.Logic.Building.PlaceBasePiece(componentInParent, component.TargetBase, CraftData.GetTechType(Builder.prefab), Builder.placeRotation);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("Logic", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Logic).GetMethod("get_Building", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_1);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(BaseGhost).GetMethod("get_TargetBase"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(CraftData).GetMethod("GetTechType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("placeRotation", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Building).GetMethod("PlaceBasePiece", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(ConstructableBase), typeof(Base), typeof(TechType), typeof(Quaternion) }, null));
                }

                if (instruction.opcode.Equals(PLACE_FURNITURE_INJECTION_OPCODE) && instruction.operand.Equals(PLACE_FURNITURE_INJECTION_OPERAND))
                {
                    /*
                     *  Multiplayer.Logic.Building.PlaceFurniture(gameObject, CraftData.GetTechType(Builder.prefab), Builder.ghostModel.transform.position, Builder.placeRotation);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("Logic", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Logic).GetMethod("get_Building", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_2);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(CraftData).GetMethod("GetTechType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("ghostModel", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(GameObject).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("placeRotation", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Building).GetMethod("PlaceFurniture", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(GameObject), typeof(TechType), typeof(Vector3), typeof(Quaternion) }, null));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
