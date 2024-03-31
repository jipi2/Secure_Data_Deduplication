from pydantic import BaseModel

class TestDto(BaseModel):
    id: int
    name: str
    