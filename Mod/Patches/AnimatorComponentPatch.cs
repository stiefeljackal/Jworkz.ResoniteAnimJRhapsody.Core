using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Patches;

using Utility;

[HarmonyPatch(typeof(Animator))]
public static class AnimatorComponentPatch
{
  private const byte MAX_SPLIT_LENGTH = 2;

  private const byte SLOT_FIELD_INDEX = 0;

  private const byte SLOT_COMP_INDEX = 0;

  private const byte COMP_FIELD_INDEX = 1;

  private static readonly Regex _componentFieldRegexSplit = new Regex(@"\.(?!.*?\.)", RegexOptions.Compiled);


  private static readonly MethodInfo _setupFieldsAsyncMethodInfo = AccessTools.Method(typeof(Animator), "SetupFieldsAsync", new[] { typeof(Func<AnimationTrack, IField>), typeof(HashSet<Slot>) });

  [HarmonyPrefix]
  [HarmonyPatch(nameof(Animator.SetupFieldsByName))]
  private static bool SetupFieldsByNamePrefix(ref Task __result, Animator __instance, Slot root)
  {
    HashSet<Slot> ignoreSlots = new();

    var setupFieldsAsyncCb = (AnimationTrack track) =>
    {
      var slot = root.FindChild((Slot s) => s.Name == track.Node && !ignoreSlots.Contains(s));
      if (slot == null) { return null; }

      var compFieldNamePair = _componentFieldRegexSplit.Split(track.Property);

      if (compFieldNamePair.Length > MAX_SPLIT_LENGTH)
      {
        throw new Exception($"Invalid Property Path: {track.Property}");
      }

      return compFieldNamePair.Length == MAX_SPLIT_LENGTH
        ? slot.GetComponentField(compFieldNamePair[SLOT_COMP_INDEX], compFieldNamePair[COMP_FIELD_INDEX])
        : slot.TryGetField(compFieldNamePair[SLOT_FIELD_INDEX]);

    };
    __result = __instance.StartTask(() => (Task)_setupFieldsAsyncMethodInfo.Invoke(__instance, new object[] { setupFieldsAsyncCb, ignoreSlots }));

    return false;
  }
}
