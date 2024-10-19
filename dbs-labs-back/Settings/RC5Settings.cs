using dbs_labs_back.Models;

namespace dbs_labs_back.Settings ;

    public class RC5Settings
    {
        public RoundCountEnum RoundCount { get; set; }

        public WordLengthInBitsEnum WordLengthInBits { get; set; }
        
        public KeyLengthInBytesEnum KeyLengthInBytes { get; set; }
        
    }