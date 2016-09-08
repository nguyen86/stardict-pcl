using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDict
{
    public class Dictionary : IDisposable
    {
        private Stream _indexStream;
        private Stream _dictStream;
        private readonly List<Entry> _cache;
        public Info Info { get; }

        public Dictionary()
        {
            Info = new Info();
            _cache = new List<Entry>();
        }

        public List<Entry> GetWords(string key)
        {
            List<Entry> results = new List<Entry>();
            if (_cache.Any())
            {
                results.AddRange(_cache.Where(x => x.WordStr.StartsWith(key, StringComparison.CurrentCultureIgnoreCase)));
            }
            using (var reader = new BinaryReader(_indexStream, Encoding.UTF8, true))
            {
                if (_indexStream.Position == 0)
                {
                    //Skip Header
                    reader.ReadInt64();
                    reader.ReadByte();
                }
                while (true)
                {
                    if (_indexStream.Position == _indexStream.Length)
                        return results;
                    var wordStr = ReadWord(reader, Encoding.UTF8);
                    var wordDataOffset = reader.ReadBytes(sizeof(Int32));
                    var wordDataSize = reader.ReadBytes(sizeof(Int32));
                    if (string.IsNullOrEmpty(wordStr))
                        return results;
                    var entry = new Entry()
                    {
                        Dictionary = this,
                        WordStr = wordStr,
                        WordDataSize = BitConverter.ToInt32(wordDataSize.Reverse().ToArray(), 0),
                        WordDataOffset = BitConverter.ToInt32(wordDataOffset.Reverse().ToArray(), 0)
                    };
                    _cache.Add(entry);
                    if (wordStr.StartsWith(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        results.Add(entry);
                    }
                    else
                    {
                        if (results.Count != 0)
                            return results;
                    }
                }
            }
        }

        public string GetExplanation(Entry entry)
        {
            if (entry == null)
                return string.Empty;
            using (var reader = new BinaryReader(_dictStream, Encoding.UTF8, true))
            {
                _dictStream.Seek(entry.WordDataOffset, SeekOrigin.Begin);
                var buffer = reader.ReadBytes(entry.WordDataSize);
                string exp = Encoding.UTF8.GetString(buffer, 0, entry.WordDataSize);
                return entry.WordStr + "\n" + exp;
            }
        }

        /// <summary>
        /// Init a dictionary by name
        /// </summary>
        /// <param name="fileReader">file reader base on your platform</param>
        /// <param name="dictName">dictionary name</param>
        public void Init(IFileReader fileReader, string dictName)
        {
            
            using (var stream = fileReader.Read(dictName + ".ifo"))
            {
                if (stream == null)
                    throw new Exception("Can not find ifo file");
                SetInfo(stream);
            }
            
            string filename;

            using (var stream = fileReader.Read(filename = dictName + ".idx") ?? fileReader.Read(filename = dictName + ".idx.dz"))
            {
                if (stream == null)
                {
                    throw new Exception("Can not find idx or idx.dz file");
                }
                SetIdxFile(filename, stream);
            }

            using (var stream = fileReader.Read(filename = dictName + ".dict") ?? fileReader.Read(filename = dictName + ".dict.dz"))
            {
                if (stream == null)
                {
                    throw new Exception("Can not find dict or dict.dz file");
                }
                SetDictFile(filename, stream);
            }
        }


        /// <summary>
        /// Init a dictionary by name
        /// </summary>
        /// <param name="fileReader">file reader base on your platform</param>
        /// <param name="dictName">dictionary name</param>
        public async Task InitAsync(IFileReader fileReader, string dictName)
        {

            using (var stream = await fileReader.ReadAsync(dictName + ".ifo"))
            {
                if (stream == null)
                    throw new Exception("Can not find ifo file");
                SetInfo(stream);
            }

            string filename;

            using (var stream = await fileReader.ReadAsync(filename = dictName + ".idx") ?? fileReader.Read(filename = dictName + ".idx.dz"))
            {
                if (stream == null)
                {
                    throw new Exception("Can not find idx or idx.dz file");
                }
                SetIdxFile(filename, stream);
            }

            using (var stream = await fileReader.ReadAsync(filename = dictName + ".dict") ?? fileReader.Read(filename = dictName + ".dict.dz"))
            {
                if (stream == null)
                {
                    throw new Exception("Can not find dict or dict.dz file");
                }
                SetDictFile(filename, stream);
            }
        }
        /// <summary>
        /// Read *.ifo file to get dictionary info
        /// </summary>  
        /// <param name="file">readable file stream</param>
        public void SetInfo(Stream file)
        {
            if (file.CanSeek)
                file.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(file, Encoding.UTF8))
            {
                do
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        return;
                    var config = line.Split('=');
                    if (config.Length == 2)
                    {
                        Info.Add(config);
                    }
                } while (true);
            }
        }

        /// <summary>
        /// Set idx file
        /// </summary>
        /// <param name="filename">input file name</param>
        /// <param name="file">readable file stream</param>
        public void SetIdxFile(string filename, Stream file)
        {
            if (Path.GetExtension(filename).EndsWith("dz"))
            {
                _indexStream = new MemoryStream(Decompress(file));
            }
            else
            {
                _indexStream = new MemoryStream();
                CopyStream(file, _indexStream);
            }

        }

        /// <summary>
        /// Set dict file
        /// </summary>
        /// <param name="filename">input file name</param>
        /// <param name="file">readable file stream</param>
        public void SetDictFile(string filename, Stream file)
        {

            if (Path.GetExtension(filename).EndsWith("dz"))
            {
                _dictStream = new MemoryStream(Decompress(file));
            }
            else
            {
                CopyStream(file, _dictStream);
            }
        }

        private void CopyStream(Stream from, Stream to)
        {
            if (from.CanSeek)
                from.Seek(0, SeekOrigin.Begin);
            from.CopyTo(to);
            if (to.CanSeek)
                to.Seek(0, SeekOrigin.Begin);
        }

        private string ReadWord(BinaryReader reader, Encoding encoding)
        {
            var sb = new StringBuilder();
            var ch = reader.ReadByte();
            while (ch > 0) //returns -1 when we've hit the end, and 0 is null
            {
                sb.Append(encoding.GetString(new[] { ch }, 0, 1));

                ch = reader.ReadByte();
            }
            return sb.ToString();
        }

        static byte[] Decompress(Stream gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(gzip, CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    memory.Seek(0, SeekOrigin.Begin);
                    return memory.ToArray();
                }
            }
        }
        public void Dispose()
        {
            _indexStream?.Dispose();
            _dictStream?.Dispose();
        }
    }
}
