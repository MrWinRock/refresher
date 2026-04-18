# Summer Jam 2026

> Refreshing

[![Unity Version](https://img.shields.io/badge/Unity-6000.3.13f1-blue)](https://unity.com/)
[![Download](https://img.shields.io/badge/Download-Itch.io-red)](https://marguro.itch.io/)

## 📖 Description

**Summer Jam 2026** Wow

## 🎮 How to Play

### Controls

* **Left Click (MB1):** Interact with ...

### Game Rules

1. **Inspect:** Wow

### Win/Lose Conditions

* ✅ **Win:** Wow
* ❌ **Lose:** Wow

## ✨ Features

### Feature 1

* **Wow:** Wow

## 🛠️ Setup

### System Requirements

* **Unity Version:** `6000.3.13f1` (Unity 6.3)
* **Platform:** Windows (PC)

### Installation

1. Clone or download this project

```bash
git clone https://github.com/yourusername/summer-jam-2026.git
```

1. Open Unity Hub
2. Click **Add** and select the project folder
3. Select Unity Editor version **6000.3.13f1**
4. Open the initial scene `Assets/Scenes/StartScene.unity`
5. Press **Play** in Unity Editor

### Build Settings

Scenes required in Build Settings (in order):

1. `StartScene.unity` - Game start screen

## 🏗️ Project Structure

```txt
```

## 🎓 Technical Details

### Core Systems

#### 1. **Shaker Minigame (Bartender QTE)**

Scripts live in `Assets/Scripts/Minigame/ShakerMinigame`.

- `ShakerMinigameController`: orchestrates flow, scoring, debug logs, and minigame handoff.
- `ShakerNoteSpawner`: controlled random spawn (no same consecutive arrow and no active-arrow conflict).
- `ShakerNoteController`: approach-circle timing and per-note lifecycle.
- `ShakerTimingJudge`: configurable `Perfect/Great/Good/Bad` windows in Inspector.
- `ShakerInputHandler`: Arrow key input + fever any-key handling.
- `ShakerUIFeedback` + `BartenderShakeAnimator`: floating judgement text and bartender shake feedback.
- `MinigameManager`: shared fever state + minigame before/after callbacks.

Quick editor test harness:

- Add `ShakerMinigameDebugBootstrap` to any active object.
- `F1` start shaker minigame
- `F2` end shaker minigame
- `F3` toggle fever mode

#### 1. **Wow**

* Wow

## 👥 Developers

* **MrWinRock** - IDK
* **LOVERnoey** - Lead Developer
* **Marguro** - Project Manager/Game Designer
* **Taki** - IDK

## 🎨 Art Design

* **zazO1** - Lead Art
* **Art2** - IDK
* **Art3** - IDK

## 📝 License

This project is for educational purposes. Please check individual asset licenses.

## 🐛 Known Issues

* Didn't start yet

---

**Download Game:** [Summer Jam 2026 on Itch.io](https://marguro.itch.io/)
