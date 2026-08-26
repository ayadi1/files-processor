namespace FilesProcessor.WebApi.Core.Exceptions;

public class FileTooLargeException(long MaxBytes, long ActualBytes) : Exception($"File size {ActualBytes} bytes exceeds the limits of {MaxBytes} bytes.")
{
}
