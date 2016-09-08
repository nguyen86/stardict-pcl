namespace StarDict
{
    public class Entry
    {
        public string WordStr { get; set; }  // a utf-8 string terminated by '\0'.
        public int WordDataOffset { get; set; }  // word data's offset in .dict file
        public int WordDataSize { get; set; }
        public Dictionary Dictionary { get; set; }
        public override string ToString()
        {
            return WordStr;
        }
    }
}
