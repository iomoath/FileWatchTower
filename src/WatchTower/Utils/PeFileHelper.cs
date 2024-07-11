using System;
using System.IO;
using PeNet;
using Serilog;

namespace WatchTower
{
    public static class PeFileHelper
    {
        public static PeFile GetPeFile(string filePath)
        {
            try
            {
                return new PeFile(filePath);
            }
            catch (ArgumentException e)
            {
                Log.Error(e, $"File '{filePath}' is not a PE. {e.Message}");
                return null;
            }
            catch (FileNotFoundException e)
            {
                Log.Error(e, $"File '{filePath}' is not found. {e.Message}");
                return null;
            }
            catch (IOException e)
            {
                Log.Error(e, $"IOException when opening the file '{filePath}'. {e.Message}");
                return null;
            }
            catch (OutOfMemoryException e)
            {
                Log.Error(e, $"OutOfMemoryException when opening the file '{filePath}'. {e.Message}");
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Error(e, $"UnauthorizedAccessException when opening the file '{filePath}'. {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, $"An exception has occurred when opening the file '{filePath}'. {e.Message}");
                return null;
            }
        }

        public static PeFile GetPeFile(byte[] fileBytes)
        {
            try
            {
                return new PeFile(fileBytes);
            }
            catch (ArgumentException e)
            {
                Log.Error(e, $"The given byte array does not represent a PE. Size: '{fileBytes?.Length}'. {e.Message}");
                return null;
            }
            catch (OutOfMemoryException e)
            {
                Log.Error(e, $"OutOfMemoryException when reading the given byte array. Size: '{fileBytes?.Length}'. {e.Message}");
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Error(e, $"UnauthorizedAccessException when reading the given byte array. Size: '{fileBytes?.Length}'. {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, $"An exception has occurred when reading the given byte array. Size: '{fileBytes?.Length}'. {e.Message}");
                return null;
            }
        }

        public static PeFile GetPeFile(Stream fileStream)
        {
            try
            {
                return new PeFile(fileStream);
            }
            catch (ArgumentException e)
            {
                Log.Error(e, $"The given file System.IO.Stream object does not represent a PE. Size: '{fileStream?.Length}'. {e.Message}");
                return null;
            }
            catch (OutOfMemoryException e)
            {
                Log.Error(e, $"OutOfMemoryException when reading The given file System.IO.Stream object. Size: '{fileStream?.Length}'. {e.Message}");
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Error(e, $"UnauthorizedAccessException when reading The given file System.IO.Stream object. Size: '{fileStream?.Length}'. {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, $"An exception has occurred when reading The given file System.IO.Stream object. Size: '{fileStream?.Length}'. {e.Message}");
                return null;
            }
        }

    }
}
