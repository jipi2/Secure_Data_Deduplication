class MTMember:
    def __init__(self, level = 0, hash = None):
        self._hash= ([0]*64) if hash is None else hash
        self._level = level
    
    def getHash(self):
        return self._hash
    
    def getLevel(self):
        return self._level