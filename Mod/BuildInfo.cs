using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyTitle(Jworkz.ResoniteAnimJRhapsody.Core.BuildInfo.Name)]
[assembly: AssemblyProduct(Jworkz.ResoniteAnimJRhapsody.Core.BuildInfo.ModId)]
[assembly: AssemblyVersion(Jworkz.ResoniteAnimJRhapsody.Core.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Jworkz.ResoniteAnimJRhapsody.Core.BuildInfo.Version)]
[assembly: AssemblyCompany("Jackalworkz")]
[assembly: InternalsVisibleTo("Jworkz.ResoniteAnimJRhapsody.Core.Test")]

namespace Jworkz.ResoniteAnimJRhapsody.Core;

public static class BuildInfo
{
  public const string Name = "[Jworkz] AnimJ Rhapsody | Core";

  public const string Author = "Stiefel Jackal";

  public const string Version = "2.0.0";

  public const string Link = "https://github.com/stiefeljackal/Jworkz.ResoniteSynctastic.Core";

  public const string ModId = $"jworkz.sjackal.{nameof(ResoniteAnimJRhapsodyCoreMod)}";
}