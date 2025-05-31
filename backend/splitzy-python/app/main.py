from fastapi import FastAPI
from app.db import database
from app.routers import users,login_router

app = FastAPI()

@app.on_event("startup")
async def startup():
    await database.connect_db()

@app.on_event("shutdown")
async def shutdown():
    await database.close_db()

app.include_router(users.router)
app.include_router(login_router.router)

