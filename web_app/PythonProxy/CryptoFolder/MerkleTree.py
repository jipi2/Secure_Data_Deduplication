class MerkleTree:
    def __init__(self):
        self._HashTree = []
        self._Levels = 0
        self._IndexOfLevel = []
        
    def getHashTree(self):
        return self._HashTree
    
    def getLevels(self):
        return self._Levels
    
    def getIndexOfLevel(self):
        return self._IndexOfLevel