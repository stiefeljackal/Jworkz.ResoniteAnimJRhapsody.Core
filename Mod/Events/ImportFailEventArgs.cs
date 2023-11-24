using System;
using FrooxEngine;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Events;

public class ImportFailEventArgs : ImportStartEventArgs
{
  public string ErrorMessage { get; }
  public  ImportFailEventArgs(User allocatingUser, string id, string fileTypeName, long byteSize, Exception e) : base(allocatingUser, id, fileTypeName, byteSize)
  {
    ErrorMessage = e.Message;
  }
}