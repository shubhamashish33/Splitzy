from fastapi import APIRouter
from app.schema.loginschema import LoginRequest, TokenPairResponse
from app.services.auth_service import authenticate_user

router = APIRouter(prefix="/auth", tags=["auth"])

@router.post("/login", response_model=TokenPairResponse)
async def login_user(login_data: LoginRequest):
    return await authenticate_user(login_data)
