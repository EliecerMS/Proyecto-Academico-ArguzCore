# ArguzCore
Proyecto hecho en C# ASP.NET Core, usando razor pages e implementado Identity

**ArguzCore** es un sistema web diseñado para centralizar y automatizar los procesos de una empresa contratista, incluyendo la gestión de proyectos, materiales, personal, maquinaria, finanzas, documentos y seguridad.

---

## 🚀 Tecnologías utilizadas

- **ASP.NET Core 8.0** (Razor Pages)
- **Entity Framework Core**
- **ASP.NET Identity**
- **SQL Server / Azure SQL Database**
- **Azure App Service** (para despliegue)
- **Enfoque de Arquitectura por capas (Clean Architecture como base)**

---

## 📂 Estructura del proyecto

```bash
ArguzCore.sln
├── ArguzCore.WebUI           # UI y presentación con Razor Pages
├── ArguzCore.Application   # Lógica de negocio (services, interfaces)
├── ArguzCore.Infrastructure # Acceso a datos (EF Core, repositorios)
└── ArguzCore.Domain        # Entidades y modelos del dominio

Otras consideraciones(edicion como prueba de corrido de pipeline):
