
# Luca - Game Of Life

## Problem Description
Conway's Game of Life is a zero-player game that follows a set of rules to determine the state of a grid of cells in each generation. The objective of this project is to implement a RESTful API that allows users to:

- Upload a board state and receive a unique identifier.

- Retrieve the next state of a given board.

- Compute multiple states ahead for a board.

- Retrieve the final stable state of a board or return an error if a stable state is not reached within a given number of iterations.

- Ensure persistence of board states even after server restarts.


## How to execute

The following commands enable the local execution of application and dependencies using Docker:

### Only dependencies
In case you want to run the code through Visual Studio (create the containers only for the dependencies):
```
cd docker
docker-compose -f docker-compose.dev.yml up -d --build
```

### Dependencies + API
In case you want to run everything using Docker (the API + dependencies):
```
cd docker
docker-compose up -d --build
```

After the successfull creation of the containers, the application will be available in [http://localhost:5010/](http://localhost:5010/).

Swagger documentation will be available on http://localhost:5010/swagger.


## Architecture
The API is designed following the Controller-Service-Repository pattern:

- **Controller (GameOfLifeController):** Handles API requests, validates input, and interacts with the service layer.

- **Service (GameOfLifeService):** Contains business logic to process board states and interact with the repository.

- **Compute Service (GameOfLifeComputeService):** Separates board state calculations, ensuring modularity and easier testing.

- **Repository (GameOfLifeRepository):** Uses Redis for storing board states, ensuring persistence across service restarts.


### Folder structure
- **docker**: contains the docker compose files
- **src**: contains the source code
- **docs**: contains documentation related files, like some sample input jsons

### Solution project division
- **GameOfLife.API**: web api project
- **GameOfLife.Tests**: contains tests

### Dependencies
- Redis
- Seq (log)

### Key Design Choices

- **State Persistence with Redis:** Ensures that board states survive service crashes or restarts.

- **Hashing for Cycle Detection:** Prevents infinite loops by detecting repeating board states.

- **Separation of Compute Logic:** `GameOfLifeComputeService` is decoupled from the main service to improve testability and maintainability.

- **Scalability Considerations:** The implementation supports large boards (500x500), but optimizations (bitwise encoding) could be introduced for improved performance.

## List of Assumptions and Trade-Offs

### Assumptions

- The board is represented as a `int[][]` where 1 represents a live cell and 0 represents a dead cell.

- The API must ensure state persistence between crashes or restarts.

- The maximum board size is 500x500 to prevent excessive memory usage.

- If a board state does not stabilize after a set number of iterations, an error should be returned.

### Trade-Offs

1. **Using Redis vs. SQL Database:**

    Pros: Fast access, easy serialization.
    
    Cons: Redis storage size grows quickly with large boards.
    
    Redis was chosen over SQL for storing board states due to its high performance, simple key-value storage, persistence, and scalability. It enables fast in-memory reads/writes, stores board states as JSON with unique GUIDs, and ensures fault tolerance through persistence options. Redis also efficiently handles multiple concurrent requests without SQL’s locking issues. SQL would be preferable for complex queries, relational data, or strict ACID compliance, but for this use case, Redis provides the best balance of speed and scalability.

2. **Full Board Storage vs. Sparse Representation:**

    Current Approach: Storing entire board as JSON in Redis.
    
    Potential Optimization: Storing only active cells or using bitwise encoding to reduce storage size.

3. **Hashing for State Detection vs. Full Board Comparison:**

    Current Approach: Using a hash function to detect repeating states quickly.
    
    Alternative: Full board comparison (slower but more accurate in rare hash collisions).
    
### Possible optimizations
- **Sparse Representation:** only active (alive) cells can be stored, instead of the entire board to save space
- **Parallel Processing:** Use multi-threading or parallel execution to compute the next state faster for large boards.
- **Caching Board States:** caching previously seen board states in Redis.
- **Use Hashes Instead of Strings:** Store board states using Redis hashes instead of JSON strings for more efficient retrieval and updates
- **Enable Redis Expiry:** Set an expiration time on stored boards to free up memory after inactivity.

- Since multiple requests can update the board state concurrently, race conditions may occur when multiple clients request the next state or final state at the same time.
To avoid race conditions, Redis transactions (MULTI/EXEC) can be used for atomic updates, optimistic locking with versioning, or distributed locking (Redlock) to prevent concurrent modifications. For large-scale applications, implement an event-driven approach (message queue) to process board updates sequentially. 