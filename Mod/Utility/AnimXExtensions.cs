using System.Collections.Generic;
using System.Linq;

using Animation = Elements.Assets.Animation;
using AnimX = Elements.Core.AnimX;
using AnimationTrack = Elements.Core.AnimationTrack;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Utility;

public static class AnimXExtensions
{
  public static Animation ToAnimation(this AnimX animx) =>
    new Animation()
    {
      GlobalDuration = animx.GlobalDuration,
      Name = animx.Name,
      Tracks = animx.GetTracks().Select((t) => t.ToAssetTrack()).ToList()
    };

  public static IEnumerable<AnimationTrack> GetTracks(this AnimX animx)
  {
    for (var i = 0; i < animx.TrackCount; i++)
    {
      yield return animx[i];
    }
  }
}
