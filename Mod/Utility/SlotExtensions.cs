using System.Linq;
using System.Text.RegularExpressions;
using Elements.Core;
using FrooxEngine;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Utility;

public static class SlotExtensions
{
  private static readonly Regex _componentFullnameMetadataRegex = new Regex(@"(?:,\s?mscorlib)(?:,\sVersion=(?:\d\.?)+)?(?:,\sCulture=\w+)?(:?,\sPublicKeyToken=\w+)?", RegexOptions.Compiled);

  public static IField GetComponentField(this Slot slot, string componentTypeName, string componentFieldName)
  {
    var components = slot.GetComponents<Component>((c) =>
      c.WorkerTypeName == componentTypeName ||
      c.WorkerType.GetNiceName() == componentTypeName ||
      _componentFullnameMetadataRegex.Replace(c.WorkerTypeName, "") == componentTypeName
    ).OrderBy(c => c.UpdateOrder).ToArray();

    var componentCount = components.Length;
    for (var i = 0; i < componentCount; i++)
    {
      var component = components[i];
      if (!component.IsDriven)
      {
        return component.TryGetField(componentFieldName);
      }
    }

    return null;
  }
}
