from fastapi import APIRouter
from typing import List
from app.schema.user import UserRead
from app.services import user_services

router = APIRouter(prefix="/users", tags=["users"])

@router.get("/", response_model=List[UserRead])
async def read_users():
    return await user_services.get_users()
