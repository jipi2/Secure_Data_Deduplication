using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib
{
    public class MTMember
    {
        public byte[] _hash { get; set; }
        public int _level { get; set; }

        public MTMember()
        {
            _hash = new byte[64];
            _level = 0;
        }

        public MTMember(int level, byte[] hash)
        {
            _hash = hash.ToArray();
            _level = level;
        }
    }
}
