namespace StarDict
{
    public class Info
    {
        public string Version { get; set; }
        public string Bookname { get; set; }      // required
        public int WordCount { get; set; }     // required
        public int SynWordCount { get; set; }  // required if ".syn" file exists.
        public int IdxFileSize { get; set; }   // required
        public int IdxOffSetBits { get; set; } // New in 3.0.0
        public string Author { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }   // You can use <br> for new line.
        public string Date { get; set; }
        public string SameTypeSequence { get; set; } // very important.

        public void Add(string[] config)
        {
            var key = config[0];
            var value = config[1];
            switch (key)
            {
                case "version":
                    Version = value;
                    break;
                case "bookname":
                    Bookname = value;
                    break;
                case "workcount":
                    WordCount = int.Parse(value);
                    break;
                case "synwordcount":
                    SynWordCount = int.Parse(value);
                    break;
                case "idxfilesize":
                    IdxFileSize = int.Parse(value);
                    break;
                case "idxoffsetbits":
                    IdxOffSetBits = int.Parse(value);
                    break;                    
                case "email":
                    Email = value;
                    break;                    
                case "website":
                    Website = value;
                    break;
                case "description":
                    Description = value;
                    break;                    
                case "date":
                    Date = value;
                    break;
                case "sametypesequence":
                    SameTypeSequence = value;
                    break;                    
            }
        }
        public Info()
        {

        }
    }
}