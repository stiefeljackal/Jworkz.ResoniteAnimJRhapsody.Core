using System.Text.Json;
using System.Threading.Tasks;
using Elements.Core;
using Elements.Assets;
using FrooxEngine;
using HarmonyLib;

using Stream = System.IO.Stream;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Patches;

using Jworkz.ResoniteAnimJRhapsody.Core.Events.Watchers;
using Readers;

[HarmonyPatch(typeof(AnimJImporter))]
internal static class AnimJImporterPatch
{
  private static readonly JsonSerializerOptions _jsonSerializerOpts = new JsonSerializerOptions
  {
    Converters = { new AnimationTrackConverter() }
  };

  [HarmonyPrefix]
  [HarmonyPatch(nameof(AnimJImporter.Import))]
  private static bool ImportPrefix(ref ValueTask<AnimX> __result, Stream stream)
  {
    var localUser = Engine.Current.WorldManager.FocusedWorld.LocalUser;

    using var reader = new AnimJReader(stream, localUser);
    AddEventHandlers(reader);
    __result = reader.DeserializeAsync(_jsonSerializerOpts);

    return false;
  }

  [HarmonyPrefix]
  [HarmonyPatch(nameof(AnimJImporter.ImportFromJSON))]
  private static bool ImportFromJSONPrefix(ref AnimX __result, string json)
  {
    var localUser = Engine.Current.WorldManager.FocusedWorld.LocalUser;

    using var reader = new AnimJReader(json, localUser);
    AddEventHandlers(reader);
    __result = reader.Deserialize(_jsonSerializerOpts);

    return false;
  }

  private static void AddEventHandlers(AnimJReader reader)
  {
    reader.ImportStart += ImporterProgressIndicatorWatcher.SpawnIndicator;
    reader.ImportProgress += ImporterProgressIndicatorWatcher.UpdateIndicator;
    reader.ImportFail += ImporterProgressIndicatorWatcher.FailIndicator;
    reader.ImportFinish += ImporterProgressIndicatorWatcher.CompleteIndicator;
  }
}
