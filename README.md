# EMLia

**Engine:** Unity 6000.0.60f1  
**Platform:** Windows  
**Language:** C#  
**Repository:** Public GitHub repository

---

## Overview

**EMLia** is a compact, first-person Unity project demonstrating a robust and modular **inventory system** with user-accessible **JSON-based save/load functionality**.  
Players can explore, **pick up**, **use**, and **drop** interactive devices, each with its own SOP (Standard Operating Procedure), while progress is persistently stored outside the game folder.  

---

## How to Run

1. **Open in Unity 6000.0.60f1 or newer**
   - Clone or download this repository.
   - Open the folder in Unity Hub.

2. **Scenes**
   - **StartMenu.unity** → main entry scene  
   - **EMLia.unity** → gameplay scene loaded by the Start button

3. **Controls**

   | Action | Key | Description |
   |:--------|:----|:-------------|
   | Move | **W A S D** | Standard FPS movement |
   | Look | Mouse | Rotate camera |
   | Pick Up / Interact | **E** | Pick up the device in front of you |
   | Use Device | **F** | Show the device’s SOP for 4 seconds |
   | Drop Device | **Q** | Return the held device to its pedestal |
   | Toggle Inventory | **Tab** | Show or hide current inventory list |
   | Save/Load Menu | **Esc** | Manual save/load & exit options |
   | Exit Game | **Esc → Exit** | Quits the app |

4. **Saving & Loading**

   All inventory saves are written as `.json` files to a user-accessible directory:
   Documents/EMLInventory/
   The three most recent saves are automatically tracked and listed by the names you give them.

---

## 📦 System Design Breakdown

### 1️⃣ Working Inventory System

**Core Features**
- Pick up, use, and drop items with persistent state.  
- JSON save/load outside the project folder.  
- Visual inventory list with collected/uncollected color coding and highlight.  
- Auto-save modal when all items are collected (or manual via Esc).  
- Load panel showing up to three most recent saves.

**Technical Approach**
- Clear **Model–View–Controller** split:
- `InventoryModel` → state logic  
- `UIController` → interface and feedback  
- `GameController` → input and player control  
- Event-driven updates (`OnAdded`, `OnCompleted`) keep systems decoupled.  
- Scene reloads restore data through `SaveLoadManager.PendingLoad`.

---

### 2️⃣ Additional System — SOP Interaction & Audio Feedback

Adds interactivity and polish:
- **F** → displays an SOP panel (device name + procedure) for 4 seconds.  
- **Q** → drops the device, respawning it at its original location and greying it out again in the inventory.  
- Integrated **audio feedback**:
- **E** (pick up) → confirmation chime + pick up sound  
- **F** (use) → activation sound  
- **Q** (drop) → return cue + drop sound  

**Inspiration & Intent**  
This system extends basic interaction with **procedural feedback**, reflecting my experience designing **XR training** applications where user actions trigger contextual instruction and sensory reinforcement.

---

### 3️⃣ Personal Element — Human-Centered Design Philosophy

This project reflects my approach as a developer: clarity of systems, meaningful feedback, and immersive learning flow.  
- The SOP system mirrors my real-world XR training work—teaching by doing.  
- Every script is modular and self-documenting for maintainability.  
- UI, sound, and interaction loops are built for usability and accessibility.  

I aim to connect **engineering precision** with **player intuition**, showing that strong system design can be both technical and human.

---

## Reflection

**EMLia** demonstrates:
- **Clarity:** Each script serves a single, well-defined role.  
- **Scalability:** New devices or SOPs can be added by extending JSON data.  
- **Creativity through structure:** Interaction and feedback replace flashy visuals with meaningful design.  
- **Individuality:** It represents how I build experiences—grounded in logic, enhanced by creativity, and centered on the user, with customized UI menus.

---

## 🗂️ Folder Overview

Assets/
├─ Scripts/
│  ├─ CoreData.cs
│  ├─ SaveLoadManager.cs
│  ├─ InventoryModel.cs
│  ├─ UIController.cs
│  ├─ GameController.cs
│  ├─ PickupInteractable.cs
│  ├─ FaceCamera.cs
│  └─ ProximityHighlighter.cs
├─ Scenes/
│  ├─ StartMenu.unity
│  └─ EMLia.unity
├─ Resources/
│  └─ DeviceCatalog.json
├─ AudioClips/
│  ├─ pickup.mp3
│  ├─ paper (use).mp3
│  └─ drop.mp3
├─ Data/
│  └─ DeviceCatalog.JSON
├─ Materials/
│  ├─ Door
│  ├─ Floor
│  ├─ Wall
│  ├─ Highlight_Mat
│  └─ Pedestal
└─ Art/
   ├─ AppLogo
   ├─ emerging_media_lab_logo
   ├─ EML_logo_transparent
   ├─ Guide
   └─ Tab_Guide

## 🏁 Summary

> **EMLia** is a concise yet complete Unity 6000.0.60f1 project that demonstrates robust data inventory management in EML (Emerging Media Lab) setting with clean architecture, responsive UI, and thoughtful user experience.  
> Players can interact with devices, view procedural SOPs, and manage inventory states that persist beyond the session through a JSON-based save system.  
> The design showcases **clarity**, **scalability**, and **individual creativity**.
>  
> This project highlights professional Unity practices, modular C# scripting, and a design philosophy grounded in **interactive learning, clean logic, and immersive feedback**.
