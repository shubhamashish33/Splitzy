from fastapi import APIRouter, Depends, HTTPException
from app.auth.jwt import decode_refresh_token, create_access_token, create_refresh_token
from app.schema.loginschema import TokenPairResponse

router = APIRouter()

@router.post("/refresh-token", response_model=TokenPairResponse)
async def refresh_token(refresh_token: str):
    payload = decode_refresh_token(refresh_token)
    if not payload:
        raise HTTPException(status_code=401, detail="Invalid refresh token")

    # Optional: check if user still exists or refresh token is valid in DB

    new_access_token = create_access_token(payload)
    new_refresh_token = create_refresh_token(payload)

    return TokenPairResponse(access_token=new_access_token, refresh_token=new_refresh_token)
