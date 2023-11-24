using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECore = Elements.Core;
using EAssets = Elements.Assets;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Utility;

public static class ElementsTrackExtensions
{
  private static readonly MethodInfo _toAssetRawTrackMethodInfo =
    typeof(ElementsTrackExtensions).GetMethod("ToAssetTrack", BindingFlags.Static, null, new Type[] { typeof(ECore.RawAnimationTrack<>) }, null);

  private static readonly MethodInfo _toAssetDiscreteTrackMethodInfo =
    typeof(ElementsTrackExtensions).GetMethod("ToAssetTrack", BindingFlags.Static, null, new Type[] { typeof(ECore.DiscreteAnimationTrack<>) }, null);

  private static readonly MethodInfo _toAssetCurveTrackMethodInfo =
    typeof(ElementsTrackExtensions).GetMethod("ToAssetTrack", BindingFlags.Static, null, new Type[] { typeof(ECore.CurveAnimationTrack<>) }, null);

  public static EAssets.RawAnimationTrack<T> ToAssetTrack<T>(this ECore.RawAnimationTrack<T> coreTrack) =>
    new EAssets.RawAnimationTrack<T>()
    {
      Interval = coreTrack.Interval,
      Node = coreTrack.Node,
      Property = coreTrack.Property,
      Keyframes = coreTrack.GetFrames().ToList()
    };

  public static EAssets.DiscreteAnimationTrack<T> ToAssetTrack<T>(this ECore.DiscreteAnimationTrack<T> coreTrack) =>
    new EAssets.DiscreteAnimationTrack<T>()
    {
      Node = coreTrack.Node,
      Property = coreTrack.Property,
      Keyframes = coreTrack.GetFrames().Select(f => f.ToAssetKeyframe()).ToList()
    };

  public static EAssets.CurveAnimationTrack<T> ToAssetTrack<T>(this ECore.CurveAnimationTrack<T> coreTrack) =>
    new EAssets.CurveAnimationTrack<T>()
    {
      Node = coreTrack.Node,
      Property = coreTrack.Property,
      Keyframes = coreTrack.GetFrames().Select(f => f.ToAssetKeyframe()).ToList()
    };

  public static EAssets.DiscreteAnimationKeyframe<T> ToAssetKeyframe<T>(this ECore.DiscreteKeyframe<T> coreKeyframe) =>
    new EAssets.DiscreteAnimationKeyframe<T>()
    {
      Time = coreKeyframe.time,
      Value = coreKeyframe.value,
    };

  public static EAssets.CurveAnimationKeyframe<T> ToAssetKeyframe<T>(this ECore.Keyframe<T> coreKeyframe) =>
    new EAssets.CurveAnimationKeyframe<T>()
    {
      Interpolation = coreKeyframe.interpolation,
      LeftTangent = coreKeyframe.leftTangent,
      RightTangent = coreKeyframe.rightTangent,
      Time = coreKeyframe.time,
      Value = coreKeyframe.value
    };

  public static EAssets.AnimationTrack ToAssetTrack(this ECore.AnimationTrack coreTrack)
  {
    var trackType = coreTrack.TrackType;
    var frameType = coreTrack.FrameType;
    MethodInfo conversionMethodInfo;

    switch (trackType)
    {
      case ECore.TrackType.Raw:
        conversionMethodInfo = _toAssetRawTrackMethodInfo.MakeGenericMethod(frameType);
        break;
      case ECore.TrackType.Discrete:
        conversionMethodInfo = _toAssetDiscreteTrackMethodInfo.MakeGenericMethod(frameType);
        break;
      case ECore.TrackType.Curve:
        conversionMethodInfo = _toAssetCurveTrackMethodInfo.MakeGenericMethod(frameType);
        break;
      default:
        throw new NotSupportedException($"Track type '{trackType}' is not supported");
    }

    return (EAssets.AnimationTrack)conversionMethodInfo.Invoke(null, new[] { coreTrack });
  }

  public static IEnumerable<T> GetFrames<T>(this ECore.RawAnimationTrack<T> coreTrack)
  {
    for (var i = 0; i < coreTrack.FrameCount; i++)
    {
      yield return coreTrack.GetFrame(i);
    }
  }

  public static IEnumerable<ECore.DiscreteKeyframe<T>> GetFrames<T>(this ECore.DiscreteAnimationTrack<T> coreTrack)
  {
    for (var i = 0; i < coreTrack.FrameCount; i++)
    {
      yield return coreTrack.GetKeyframe(i);
    }
  }

  public static IEnumerable<ECore.Keyframe<T>> GetFrames<T>(this ECore.CurveAnimationTrack<T> coreTrack)
  {
    for (var i = 0; i < coreTrack.FrameCount; i++)
    {
      yield return coreTrack.GetKeyframe(i);
    }
  }
}
