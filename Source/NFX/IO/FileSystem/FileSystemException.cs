using System;
using System.Runtime.Serialization;

namespace NFX.IO.FileSystem
{
  /// <summary>
  /// General NFX file system specific exception
  /// </summary>
  public class FileSystemException: NFXException
  {
    public FileSystemException()
    {
    }

    public FileSystemException(string message)
      : base(message)
    {
    }

    public FileSystemException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected FileSystemException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  } //FileSystemException
}
