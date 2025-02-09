# Usar la imagen oficial de Python
FROM python:3.11

# Establecer el directorio de trabajo dentro del contenedor
WORKDIR /app

# Copiar los archivos del proyecto
COPY . .

# Instalar dependencias
RUN pip install --no-cache-dir fastapi uvicorn motor

# Exponer el puerto en el que correrá FastAPI
EXPOSE 8000

# Comando para ejecutar la API
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
