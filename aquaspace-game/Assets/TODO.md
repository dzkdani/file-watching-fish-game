##TODO
- rework the food to use object pooling instead of instantiate/destroy
- make hungry fish keep moving not idle when hunger is 0, but instead actively seek food
- make a prefabs of fish, trash, and food with the relevant components and settings, and spawn those instead of empty gameobjects
- separeate spawnsystem into spawnarea and spawnmanager, where spawnarea is responsible for spawning objects in a specific area, and spawnmanager is responsible for managing the spawnareas and spawning objects based on the game state
- rework the movement using DOTween instead of lerping in update, to make it smoother and more efficient