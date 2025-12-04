# ⚓ Battleship: Naval Warfare

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Language](https://img.shields.io/badge/language-C%23-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)


## Made by
**Omayma El Kasbaoui** and **Timothé Chiesi** 

---

> **Battleship: Naval Warfare** is a feature-rich, digital recreation of the classic naval combat board game.

Built with **C#**, this project moves beyond simple coordinate guessing. It emphasizes tactical freedom, offering dynamic grid sizing, smart AI opponents tailored to your skill level, and competitive multiplayer modes.

---

## ✨ Key Features

* **🧠 Smart AI:** Play against an AI that hunts, targets, and predicts.
* **📏 Dynamic Grids:** Why stick to 10x10? Customize your map size for quicker skirmishes or epic wars.
* **⏪ Tactical Rollback:** Made a mistake? Utilize the move-history feature to rollback the game state to a previous turn.
* **⚔️ PvP Multiplayer:** Challenge a friend in a classic hot-seat environment.

---

## 🎮 Game Modes

### 1. Single Player (Human vs. AI)
Test your mettle against the computer. The AI isn't just random; it reacts to the state of the board.

| Difficulty | Description |
| :--- | :--- |
| **Cadet (Easy)** | The AI fires randomly. Perfect for beginners learning the mechanics. |
| **Admiral (Hard)** | The AI utilizes **probability maps** and **hunting algorithms**. Once it scores a hit, it calculates the most likely location of the rest of your ship to sink you efficiently. |

### 2. Multiplayer (PvP)
Challenge a friend on the same machine!
* **Classic Turn-Based:** Players take turns inputting coordinates while the other looks away (Hot-seat mode).

---

## 🕹️ How to Play

### 1. Setup
* **Main Menu:** Select "Single Player" or "Multiplayer".
* **Grid Configuration:** Input your desired grid size (Standard is `10`).
* **Difficulty:** Select `Cadet` or `Admiral`.

### 2. Deployment
You must place 5 ships. You will be asked for a **Coordinate** (e.g., `A1`) and an **Orientation** (Horizontal/Vertical). To move your ship you must drag it and drop it where you want. If you want to rotate it, simply click on it.

* 🚢 **Carrier** (4 cells)
* 🛳️ **Battleship** (3 cells)
* 🛳️ **Battleship** (3 cells)
* 🛥️ **Cruiser** (2 cells)
* 🛥️ **Cruiser** (2 cells)
* ⛵ **Destroyer** (1 cells)



### 3. Battle
Once the game starts, you will take turns firing shots.
* **Input Format:** Click on the opponent grids to choose where to attack.
* **Feedback:** The status section will show you what's the last thing happend.
* **Winning:** The first player to sink all opposing ships claims victory!

