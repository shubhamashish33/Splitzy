from pydantic import BaseModel
from datetime import datetime

class UserRead(BaseModel):
    user_id: int
    name: str
    email: str
    created_at: datetime
