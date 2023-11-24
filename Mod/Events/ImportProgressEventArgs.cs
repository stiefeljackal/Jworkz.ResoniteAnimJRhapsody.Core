using System;
using FrooxEngine;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Events;

public class ImportProgressEventArgs : ImportStartEventArgs
{
  public long ReadSize { get; }

  public float Percent => (float)Math.Round(ReadSize / (double)ByteSize * 100d);

  public  ImportProgressEventArgs(User allocatingUser, string id, string fileTypeName, long byteSize, long readSize) : base(allocatingUser, id, fileTypeName, byteSize)
  {
    ReadSize = readSize;
  }
}