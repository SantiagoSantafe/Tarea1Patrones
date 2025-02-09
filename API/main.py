from fastapi import FastAPI
from motor.motor_asyncio import AsyncIOMotorClient
from pydantic import BaseModel
import os

app = FastAPI()

# Obtener la URI de MongoDB desde la variable de entorno
MONGO_URI = os.getenv("MONGO_URI", "mongodb://localhost:27017")

# Conectar a MongoDB
client = AsyncIOMotorClient(MONGO_URI)
db = client.mibase  # Base de datos "mibase"
collection = db.nombres  # Colección "nombres"

# Modelo de datos
class NombreModel(BaseModel):
    nombre: str

@app.get("/")
async def leer_nombres():
    """Obtener todos los nombres de la base de datos."""
    nombres = await collection.find().to_list(100)
    return {"nombres": [n["nombre"] for n in nombres]}

@app.post("/")
async def agregar_nombre(nombre: NombreModel):
    """Agregar un nuevo nombre a la base de datos."""
    await collection.insert_one({"nombre": nombre.nombre})
    return {"mensaje": f"Nombre {nombre.nombre} agregado"}
