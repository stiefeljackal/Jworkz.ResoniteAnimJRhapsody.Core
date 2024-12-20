using System;
using FrooxEngine;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Events;

public class ImportProgressEventArgs : ImportStartEventArgs
{
  public long ReadSize { get; }

  public float Percent => (float)ReadSize / ByteSize;

  public  ImportProgressEventArgs(User allocatingUser, string id, string fileTypeName, long byteSize, long readSize) : base(allocatingUser, id, fileTypeName, byteSize)
  {
    ReadSize = readSize;
  }
}