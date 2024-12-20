using System;
using System.Collections.Concurrent;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;
using SkyFrost.Base;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Events.Watchers;

internal static class ImporterProgressIndicatorWatcher
{
  internal const float SPAWN_DISTANCE = 0.05f;

  internal static ConcurrentDictionary<string, RefID?> IdToIndicatorDictionary = new();

  internal static ConcurrentDictionary<RefID?, string> IndicatorToIdDictionary = new();

  internal static ConcurrentDictionary<string, Action> WaitingIndicatorAction = new();

  public static bool IsWatchingImport(string importId) => IdToIndicatorDictionary.ContainsKey(importId);

  public static void SpawnIndicator(object _, ImportStartEventArgs args)
  {
    var world = args.World;
    var user = args.AllocatingUser;
    var id = args.Id;
    var fileTypeName = args.FileTypeName;

    user.GetPointInFrontOfUser(out float3 spawnPoint, out floatQ rotation);

    world.RunSynchronously(async () =>
    {
      var indicatorSlotName = $"Import {fileTypeName} Indicator";
      var indicatorSlot = user.LocalUserSpace.AddSlot(indicatorSlotName, false);
      indicatorSlot.OnPrepareDestroy += OnSlotPrepareDestroy;
      indicatorSlot.AttachComponent<DestroyOnUserLeave>().TargetUser.Target = world.LocalUser;
      indicatorSlot.PositionInFrontOfUser(spawnPoint, distance: SPAWN_DISTANCE);
      IdToIndicatorDictionary.TryAdd(id, null);

      await indicatorSlot.StartGlobalTask(async () =>
      {
        if (!IdToIndicatorDictionary.ContainsKey(id)) { return; }

        var indicator = await indicatorSlot.SpawnEntity<ProgressBarInterface, LegacySegmentCircleProgress>(FavoriteEntity.ProgressBar);
        indicator.Initialize(false);
        indicatorSlot.Name = indicatorSlotName;

        var grabbable = indicatorSlot.GetComponent<Grabbable>();
        if (grabbable != null)
        { 
          grabbable.Enabled = false;
        }
        indicator.UpdateProgress(0f, $"Importing {fileTypeName}", string.Empty);
        IdToIndicatorDictionary.TryUpdate(id, indicator.ReferenceID, null);
        IndicatorToIdDictionary.TryAdd(indicator.ReferenceID, id);

        var hasWaitingAction = WaitingIndicatorAction.TryRemove(id, out Action waitingAction);
        if (!hasWaitingAction) { return; }

        waitingAction();
      });
    }, true);
  }

  public static void UpdateIndicator(object _, ImportProgressEventArgs args)
  {
    var indicator = GetProgressIndicator(args.Id, args.World);
    args.World.RunSynchronously(() =>
    {
      indicator?.UpdateProgress(args.Percent, $"Importing {args.FileTypeName}", $"{args.ReadSize} / {args.ByteSize}");
    }, true);
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
    var resolveAction = () =>
    {
      var indicator = GetProgressIndicator(importId, world);

      indicator?.UpdateProgress(0f, indicator.SubPhaseName.Target.Value, string.Empty);
      RemoveIndicatorCache(importId);

      if (indicator == null) { return; }

      if (!isError)
      {
        indicator.ProgressDone($"{fileTypeName} has been imported!");
      }
      else
      {
        indicator.ProgressFail($"Failed to import {fileTypeName}: {message}");
      }

    };

    if (HasProgressIndicator(importId, world, out ProgressBarInterface _))
    {
      world.RunSynchronously(resolveAction, true);
    }
    else
    {
      WaitingIndicatorAction.TryAdd(importId, resolveAction);
    }
  }

  private static bool HasProgressIndicator(string importId, World world, out ProgressBarInterface indicator)
  {
    var hasRefId = IdToIndicatorDictionary.TryGetValue(importId, out var refId);

    indicator = hasRefId && refId.HasValue
      ? world.ReferenceController.GetObjectOrNull(refId.Value) as ProgressBarInterface
      : null;

    return indicator != null;
  }

  private static ProgressBarInterface GetProgressIndicator(string importId, World world)
  {
    var hasProgresIndicator = HasProgressIndicator(importId, world, out var progressBar);
    return hasProgresIndicator ? progressBar : null;
  }

  private static void OnSlotPrepareDestroy(Slot slot)
  {
    var indicator = slot.GetComponent<ProgressBarInterface>();
    RemoveIndicatorCache(indicator.ReferenceID);
  }

  private static void RemoveIndicatorCache(RefID refId)
  {
    if (!IndicatorToIdDictionary.ContainsKey(refId)) { return; }

    IndicatorToIdDictionary.TryRemove(refId, out string importId);
    IdToIndicatorDictionary.TryRemove(importId, out RefID? _);
  }

  private static void RemoveIndicatorCache(string importId)
  {
    if (!IdToIndicatorDictionary.ContainsKey(importId)) { return; }

    IdToIndicatorDictionary.TryRemove(importId, out RefID? refId);
    IndicatorToIdDictionary.TryRemove(refId, out string _);
  }
}
