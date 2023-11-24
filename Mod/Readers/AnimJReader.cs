using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Elements.Core;
using Elements.Assets;
using FrooxEngine;

using Stream = System.IO.Stream;
using Animation = Elements.Assets.Animation;

namespace Jworkz.ResoniteAnimJRhapsody.Core.Readers;

using Events;

public class AnimJReader : IDisposable
{
  private const int MIN_BYTES_TO_REPORT = 210000;

  private const string FILE_TYPE = "AnimJ";

  private const byte START_AT_SECOND = 7;

  private const byte REPORT_INTERNAL_IN_SECONDS = 2;

  private Stream _stream;

  private User _allocatingUser;

  private Timer _progressTimer;
  
  public event EventHandler<ImportStartEventArgs> ImportStart;

  public event EventHandler<ImportProgressEventArgs> ImportProgress;

  public event EventHandler<ImportFinishEventArgs> ImportFinish;

  public event EventHandler<ImportFailEventArgs> ImportFail;

  public long BufferLength => _stream.Length;

  public long ConsumedBytes => _stream.Position;

  public string ImportId { get; private set; }

  public bool IsDisposed { get; private set; }

  private AnimJReader(User allocatingUser)
  {
    _allocatingUser = allocatingUser;
    ImportId = $"{FILE_TYPE}.{allocatingUser.UserID}.{DateTime.Now}";
  }

  public AnimJReader(string json, User allocatingUser) : this(allocatingUser)
  {
    _stream = new MemoryStream();

    var writer = new StreamWriter(_stream);
    writer.Write(json);
    _stream.Position = 0;
  }

  public AnimJReader(Stream stream, User allocatingUser) : this(allocatingUser)
  {
    _stream = stream;
  }

  public AnimX Deserialize(JsonSerializerOptions options)
  {
    Start();

    try
    {
      var animation = JsonSerializer.Deserialize<Animation>(_stream, options);
      return Finish(animation);
    }
    catch (Exception ex)
    {
      OnImportFail(ex);
    }

    return null;
  }

  public async ValueTask<AnimX> DeserializeAsync(JsonSerializerOptions options)
  {
    Start();

    try
    {
      var animation = await JsonSerializer.DeserializeAsync<Animation>(_stream, options);
      return Finish(animation);

    }
    catch (Exception ex)
    {
      OnImportFail(ex);
    }

    return null;
  }


  private void Start()
  {
    if (BufferLength < MIN_BYTES_TO_REPORT) { return; }

    _progressTimer = new Timer((object _) =>
    {
      ImportProgress?.Invoke(this, new ImportProgressEventArgs(_allocatingUser, ImportId, FILE_TYPE, BufferLength, ConsumedBytes));
    }, FILE_TYPE, TimeSpan.FromSeconds(START_AT_SECOND), TimeSpan.FromSeconds(REPORT_INTERNAL_IN_SECONDS));

    ImportStart?.Invoke(this, new ImportStartEventArgs(_allocatingUser, ImportId, FILE_TYPE, BufferLength));
  }

  private AnimX Finish(Animation animation)
  {
    ImportFinish?.Invoke(this, new ImportFinishEventArgs(_allocatingUser, ImportId, FILE_TYPE, BufferLength));

    return AnimJImporter.CreateFrom(animation);
  }

  private void OnImportFail(Exception ex)
  {
    ImportFail?.Invoke(this, new ImportFailEventArgs(_allocatingUser, ImportId, FILE_TYPE, BufferLength, ex));
  }

  public override int GetHashCode() => _stream?.GetHashCode() ?? 0;

  public void Dispose()
  {
    if (IsDisposed) { return; }

    _progressTimer?.Dispose();
    _stream.Dispose();
    ImportFail = null;
    ImportFinish = null;
    ImportProgress = null;
    ImportStart = null;

    IsDisposed = true;
  }
}
