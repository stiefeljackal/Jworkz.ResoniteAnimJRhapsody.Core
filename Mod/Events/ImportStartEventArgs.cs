using FrooxEngine;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Events;

public class ImportStartEventArgs
{
  public World World => AllocatingUser?.World;

  public User AllocatingUser { get; }

  public string FileTypeName { get; }

  public string Id { get; }

  public long ByteSize { get; }

  public  ImportStartEventArgs(User allocatingUser, string id, string fileTypeName, long byteSize)
  {
    AllocatingUser = allocatingUser;
    Id = id;
    FileTypeName = fileTypeName;
    ByteSize = byteSize;
  }
}