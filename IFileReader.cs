using System.IO;
using System.Threading.Tasks;

namespace StarDict
{
    public interface IFileReader
    {
        /// <summary>
        /// Read file to Stream
        /// </summary>
        /// <param name="filename">file path</param>
        /// <returns>file stream or null if file not exist</returns>
        Stream Read(string filename);

        /// <summary>
        /// Read file to Stream asynchronous
        /// </summary>
        /// <param name="filename">file path</param>
        /// <returns>file stream or null if file not exist</returns>
        Task<Stream> ReadAsync(string filename);
    }
}