# 🛡️ Proyecto Escudo PromElec - Modernización de Backend API

Este proyecto consiste en la modernización, aseguramiento y despliegue de la API RESTful de PromElec. Se ha implementado una arquitectura robusta utilizando .NET 8, seguridad avanzada, pruebas automatizadas y contenedorización con Docker.

## 🚀 Características Principales
* **Arquitectura:** Implementación del **Repository Pattern** para desacoplar la lógica de datos.
* **Seguridad:** Autenticación y Autorización mediante **JWT (JSON Web Tokens)**.
* **Testing:**
    * 10 Pruebas Unitarias (xUnit + Moq).
    * 20 Pruebas de Integración (WebApplicationFactory).
* **Despliegue:** Dockerización optimizada con soporte para Base de Datos en Memoria automática.

---

## 📋 Requisitos Previos
* .NET 8 SDK
* Docker Desktop
* SQL Server (Opcional para entorno local, no necesario para Docker).

---

## 🛠️ Instrucciones de Instalación y Ejecución Local

1.  **Clonar o descomprimir el proyecto:**
    Navega a la carpeta raíz del proyecto.

2.  **Restaurar dependencias:**
    ```bash
    dotnet restore
    ```

3.  **Ejecutar la API:**
    ```bash
    dotnet run --project TiendaPromElec
    ```

4.  **Acceder a Swagger:**
    Abre tu navegador en la dirección que indique la consola (usualmente `http://localhost:5152/swagger`).

> **Nota:** En ejecución local, la API intentará conectarse a SQL Server usando la cadena de conexión en `appsettings.json`.

---

## 🧪 Ejecución de Pruebas (Testing)

El proyecto cuenta con una suite completa de pruebas para asegurar la calidad del código.

Para ejecutar **todas** las pruebas (Unitarias e Integración) simultáneamente:

```bash
dotnet test
