using System.Collections.Concurrent;
using Elements.Core;
using SkyFrost.Base;
using FrooxEngine;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Events.Watchers;

internal static class ImporterProgressIndicatorWatcher
{
  internal const float SPAWN_DISTANCE = 0.05f;

  internal static ConcurrentDictionary<string, RefID> IdToIndicatorDictionary = new();

  internal static ConcurrentDictionary<RefID, string> IndicatorToIdDictionary = new();

  public static bool IsWatchingImport(string importId) => IdToIndicatorDictionary.ContainsKey(importId);

  public static void SpawnIndicator(object _, ImportStartEventArgs args)
  {
    var world = args.World;
    var user = args.AllocatingUser;
    var id = args.Id;
    var fileTypeName = args.FileTypeName;

    world.RunSynchronously(async () =>
    {
      user.GetPointInFrontOfUser(out float3 spawnPoint, out floatQ rotation);

      var indicatorSlot = user.LocalUserSpace.AddSlot($"Import {fileTypeName} Indicator", false);
      indicatorSlot.PositionInFrontOfUser(spawnPoint, distance: SPAWN_DISTANCE);

      var indicator = await indicatorSlot.SpawnEntity<ProgressBarInterface, LegacySegmentCircleProgress>(FavoriteEntity.ProgressBar);
      indicatorSlot.AttachComponent<DestroyOnUserLeave>().TargetUser.Target = world.LocalUser;

      var localUserGlobalPos = user.LocalUserSpace.GlobalPosition;
      indicator.UpdateProgress(0f, $"Importing {fileTypeName}", string.Empty);

      IdToIndicatorDictionary.TryAdd(id, indicator.ReferenceID);
      IndicatorToIdDictionary.TryAdd(indicator.ReferenceID, id);

      indicatorSlot.OnPrepareDestroy += OnSlotPrepareDestroy;
    }, true);
  }

  public static void UpdateIndicator(object _, ImportProgressEventArgs args)
  {
    var indicator = GetProgressIndicator(args.Id, args.World);
    indicator?.UpdateProgress(args.Percent, $"Importing {args.FileTypeName}", $"{args.ReadSize} / {args.ByteSize} ({args.Percent}%)");
  }

  public static void CompleteIndicator(object _, ImportFinishEventArgs args)
  {
    ResolveIndicator(args.Id, args.World, args.FileTypeName);
  }

  public static void FailIndicator(object _, ImportFailEventArgs args)
  {
    ResolveIndicator(args.Id, args.World, args.FileTypeName, true, args.ErrorMessage);
  }

  private static void ResolveIndicator(string importId, World world, string fileTypeName, bool isError = false, string message = "")
  {
    var indicator = GetProgressIndicator(importId, world);

    if (indicator == null) { return; }

    if (!isError)
    {
      indicator.ProgressDone($"{fileTypeName} has been imported!");
    }
    else
    {
      indicator.ProgressFail($"Failed to import {fileTypeName}: {message}");
    }

    RemoveIndicatorCache(indicator.ReferenceID);
  }

  private static ProgressBarInterface GetProgressIndicator(string importId, World world)
  {
    var hasRefId = IdToIndicatorDictionary.TryGetValue(importId, out var refId);
    return hasRefId ? world.ReferenceController.GetObjectOrNull(refId) as ProgressBarInterface : null;
  }

  private static void OnSlotPrepareDestroy(Slot slot)
  {
    var indicator = slot.GetComponent<ProgressBarInterface>();
    RemoveIndicatorCache(indicator.ReferenceID);
  }

  private static void RemoveIndicatorCache(RefID refId)
  {
    IndicatorToIdDictionary.TryRemove(refId, out string importId);
    IdToIndicatorDictionary.TryRemove(importId, out RefID _);
  }
}
