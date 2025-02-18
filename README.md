# Gem Match 3
#### Video Demo: https://youtu.be/O_jMCPU7joA
#### Description:
This Match-3 game, developed in Unity using C#, challenges players to match items on a grid, utilize powerful boosters, and progress through various levels. The game includes unique mechanics such as blocking items, generating items, and special events that add depth and strategy to the gameplay.

## Core Mechanics

### Matchable Items
- **Main Function:** Matchable items are the primary elements in the game. Players must align three or more identical items vertically or horizontally to create a match.
- **Interactions:** Matching items may interact with nearby blocking and generating items, causing various effects.

### Blocking Items
- **Function:** Blocking items serve as obstacles, preventing other items from falling or being matched.
- **Destruction:** These items can be destroyed if a match occurs adjacent to them, clearing the path for other items to drop down.

### Generating Items
- **Function:** Generating items produce target items when a match occurs nearby. These target items are essential for completing level objectives.
- **Interaction:** By activating generating items, players can produce the necessary items to reach their goals.

## Booster Mechanics

### In-Game Boosters
These boosters appear as items within the levels and can be combined for enhanced effects:

1. **TNT** - Explodes in a small area.
2. **Lightball** - Clears all items of a specific type.
3. **Vertical Rocket** - Clears an entire column.
4. **Horizontal Rocket** - Clears an entire row.
5. **Missile** - Targets and destroys a specific item or obstacle.

### UI Boosters
These boosters can be selected via the game's UI and used during a level:

1. **Hammer** - Destroys a single item or obstacle.
2. **Jester Hat** - Shuffles the board, rearranging all items.
3. **Bow** - Shoots arrows at selected items, clearing them.
4. **Cannon** - Clears a large area of the board with a powerful blast.

### Combining Boosters
- **Combination Effects:** Players can combine two or more boosters on the board to create unique and powerful effects. For example, combining a Vertical Rocket with a Lightball clears both a row and all items of a specific type.

## Events

### Main Event
- **Progression:** The main event is tied to the destruction of a specific item type within levels.
- **Rewards:** As players destroy the target items, they fill a goal bar. Once the goal is met, players receive rewards.

### Side Event
- **Mechanics:** Side events feature unique gameplay mechanics. Players earn rights to participate by completing levels.
- **Rewards:** These rights can be used to participate in side events and win additional rewards.

## In-Game Shop
- **Function:** The shop allows players to purchase boosters and other items to aid their progress.
- **Note:** The shop does not support real-money transactions as there is no Google backend connection. Players use in-game currency for purchases.

## Winning and Losing

### Objectives
- Each level has specific objectives, such as clearing a certain number of matchable items or destroying blocking items.

### Completion
- Players win a level by completing the objectives within a limited number of moves.
- Failing to complete the objectives within the given moves results in a loss.

AWS Integration, Shifter Cells and Obstacles Video

https://github.com/Rimaethon/Gem-Match3/assets/44122638/fc50b6ac-7425-4d36-b8e7-5b635bc43b9f


Gameplay Video

https://github.com/Rimaethon/Gem-Match3/assets/44122638/76b6f48b-fad1-4b65-b874-c9a646a2be4a



Level Design Tools


https://github.com/Rimaethon/Gem-Match3/assets/44122638/0a3f3b87-5178-4dec-a6a6-e9dc98f582c8
