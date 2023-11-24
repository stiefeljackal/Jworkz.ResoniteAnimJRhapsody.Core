using FrooxEngine;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Events;

public class ImportFinishEventArgs : ImportStartEventArgs
{
  public  ImportFinishEventArgs(User allocatingUser, string id, string fileTypeName, long byteSize) : base(allocatingUser, id, fileTypeName, byteSize) { }
}