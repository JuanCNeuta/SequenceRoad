# SequenceRoad

**Juego educativo (Unity) — proyecto de tesis**

**Resumen corto:**
SequenceRoad es un juego serio/educativo desarrollado en Unity pensado para enseñar y evaluar habilidades de **pensamiento computacional** (énfasis en *algoritmia*) a niños con Trastorno del Espectro Autista (TEA) nivel 1. Fue diseñado como parte de la tesis y contiene múltiples niveles con una interfaz consistente pero contenidos y mapas distintos.

---

# Contenido del README
1. Descripción
2. Características principales
3. Objetivos pedagógicos
4. Requisitos (software / paquetes)
5. Instalación y ejecución (desarrollo)
6. Instrucciones de build (Windows / Android)
7. Licencia y créditos

---

# 1. Descripción
SequenceRoad guía al jugador (niño) por una serie de niveles en los que debe ordenar o seleccionar acciones para llevar un avatar desde un punto A hasta un punto B. Cada nivel está diseñado para medir y entrenar componentes del pilar de **algoritmia** del pensamiento computacional.

El proyecto incluye:
- Múltiples niveles con la misma interfaz y mecánicas base..

# 2. Características principales
- Diseñado para población infantil con TEA nivel 1.
- Niveles progresivos (dificultad incremental).
- Interfaz sencilla y consistente entre niveles.
- Integración con TextMeshPro y Lottie para animaciones UI.

# 3. Objetivos pedagógicos
- Practicar la formación y ejecución de secuencias (algoritmos simples).
- Mejorar la resolución de problemas y la planificación secuencial.
- Proveer datos cuantitativos para evaluar el rendimiento por nivel.

# 4. Requisitos
- Unity (recomendado): **Unity 6000.0.33f1**.
- Paquetes Unity: `com.unity.textmeshpro` (TextMeshPro).
- Assets externos: `LottiePlugin.UI` (usado en algunos scripts).

# 5. Instalación y ejecución (desarrollo)
```bash
# clona el repositorio
git clone https://github.com/JuanCNeuta/SequenceRoad.git
cd SequenceRoad
```
1. Abre Unity Hub.
2. Selecciona "Add" y apunta a la carpeta del proyecto clonada.
3. Abre el proyecto con la versión de Unity recomendada.
4. En la ventana de `Scenes` (Assets/Scenes/) abre la escena principal `StartMenu.unity`.
5. Presiona Play en el Editor para ejecutar localmente.

# 6. Instrucciones de build
## Windows (Standalone)
- File → Build Settings → Seleccionar "PC, Mac & Linux Standalone" → Build.
- Seleccionar la escena(s) principal(es) y configurar opciones de resolución.

## Android
- Instalar Android Build Support en Unity Hub (SDK/NDK/OpenJDK).
- File → Build Settings → Android → Switch Platform → Build.
- Firmar con el certificado si vas a publicar.

# 7. Licencia y créditos
- **Licencia:**

```
MIT License
Copyright (c) 2025 Juan Carlos Neuta Montenegro y William Caicedo Magin
```

- **Créditos:**
  - Autor: Juan Carlos Neuta Montenegro y William Caicedo Magin
  - Basado en trabajo de tesis sobre gamificación y pensamiento computacional para población TEA.
  - Plugins/Assets: TextMeshPro, LottiePlugin.UI, otros assets usados.

---
