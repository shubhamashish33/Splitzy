from typing import List
from app.db.database import get_pool
from app.schema.user import UserRead

async def get_users() -> List[UserRead]:
    pool = get_pool()
    query = "SELECT user_id, name, email, created_at FROM users"
    async with pool.acquire() as conn:
        rows = await conn.fetch(query)
        return [UserRead(**dict(row)) for row in rows]
