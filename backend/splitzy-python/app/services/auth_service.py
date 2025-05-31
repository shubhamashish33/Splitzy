from app.auth.jwt import create_access_token, create_refresh_token
from app.schema.loginschema import LoginRequest, TokenPairResponse
from fastapi import HTTPException
from app.db.database import get_pool

async def authenticate_user(login_data: LoginRequest) -> TokenPairResponse:
    pool = get_pool()
    async with pool.acquire() as conn:
        row = await conn.fetchrow(
            "SELECT user_id, email, password_hash FROM users WHERE email = $1",
            login_data.email
        )
        if not row:
            raise HTTPException(status_code=400, detail="Invalid credentials")

        # Add password check here
        # if not bcrypt.checkpw(login_data.password.encode(), row["password_hash"].encode()):
        #     raise HTTPException(status_code=400, detail="Invalid credentials")

        payload = {"sub": str(row["user_id"]), "email": row["email"]}
        access_token = create_access_token(payload)
        refresh_token = create_refresh_token(payload)

        return TokenPairResponse(access_token=access_token, refresh_token=refresh_token)
