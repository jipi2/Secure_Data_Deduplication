using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib
{
    public class MerkleTree
    {
        public List<MTMember> HashTree { get; set; }
        public int Levels { get; set; }
        public List<int> IndexOfLevel { get; set; }

        public MerkleTree()
        {
            HashTree = new List<MTMember>();
            IndexOfLevel = new List<int>();
            Levels = 0;
        }
    }
}
