import asyncpg
from app.core.config import DATABASE_URL

_pool = None

async def connect_db():
    global _pool
    _pool = await asyncpg.create_pool(DATABASE_URL)

async def close_db():
    global _pool
    await _pool.close()

def get_pool():
    if _pool is None:
        raise RuntimeError("Database pool is not initialized")
    return _pool
