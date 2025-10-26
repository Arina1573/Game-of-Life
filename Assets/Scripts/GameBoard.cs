using UnityEngine;

using UnityEngine.Tilemaps;

using System.Collections;

using System.Collections.Generic;

using UnityEngine.EventSystems;



public class GameBoard : MonoBehaviour

{

    [Header("Tilemap References")]

    [SerializeField] private Tilemap currentState;

    [SerializeField] private Tilemap nextState;

    [SerializeField] private Tile aliveTile;

    [SerializeField] private Tile deadTile;

    

    [Header("Game Settings")]

    [SerializeField] private float updateInterval = 0.05f;

    [SerializeField] private Vector2Int boardSize = new Vector2Int(50, 50);

    [SerializeField] private Vector2Int boardCenter = Vector2Int.zero;



    private HashSet<Vector3Int> aliveCells;

    private HashSet<Vector3Int> cellsToCheck;

    private bool isRunning = false;

    private Coroutine gameLoop;

    private BoundsInt boardBounds;



    public Vector2Int BoardSize => boardSize;

    public BoundsInt Bounds => boardBounds;

    public bool IsRunning => isRunning;



    private void Awake()

    {

        aliveCells = new HashSet<Vector3Int>();

        cellsToCheck = new HashSet<Vector3Int>();

        CalculateBoardBounds();

    }



    private void Start()

    {

        Clear();

        aliveCells.Clear();

        InitializeBoard(currentState);

    }



    private void CalculateBoardBounds()

{

        Vector3Int min = new Vector3Int(

            boardCenter.x - boardSize.x / 2,

            boardCenter.y - boardSize.y / 2, 

            0

        );

        Vector3Int max = new Vector3Int(

            boardCenter.x + boardSize.x / 2,

            boardCenter.y + boardSize.y / 2,

            0

        );

    boardBounds = new BoundsInt(min, max - min + Vector3Int.one);

    }



    private void InitializeBoard(Tilemap state)

    {

        // Инициализируем все клетки как мертвые

        for (int x = boardBounds.xMin; x < boardBounds.xMax; x++)

        {

            for (int y = boardBounds.yMin; y < boardBounds.yMax; y++)

            {

                Vector3Int cell = new Vector3Int(x, y, 0);

                state.SetTile(cell, deadTile);

            }

        }

    }



    public void StartSimulation()

    {

        if (!isRunning)

        {

            isRunning = true;

            gameLoop = StartCoroutine(GameLoop());

        }

    }



    public void StopSimulation()

    {

        if (isRunning)

        {

            isRunning = false;

            if (gameLoop != null)

                StopCoroutine(gameLoop);

        }

    }



    public void ClearBoard()

    {

        StopSimulation();

        Clear();

        aliveCells.Clear();

        

        // Переинициализируем сетку

        InitializeBoard(currentState);

    }



    public void SetRandomPattern()

    {

        ClearBoard();

        for (int x = boardBounds.xMin; x < boardBounds.xMax; x++)

        {

            for (int y = boardBounds.yMin; y < boardBounds.yMax; y++)

            {

                if (Random.value > 0.85f)

                {

                    Vector3Int cell = new Vector3Int(x, y, 0);

                    currentState.SetTile(cell, aliveTile);

                    aliveCells.Add(cell);

                }

            }

        }

    }



    public void SetUpdateSpeed(float speed)

    {

        updateInterval = Mathf.Clamp(1f / speed, 0.01f, 1f);

    }



    private IEnumerator GameLoop()

    {

        while (isRunning)

        {

            UpdateState();

            yield return new WaitForSeconds(updateInterval);

        }

    }



    private void UpdateState()

    {

        cellsToCheck.Clear();

        InitializeBoard(nextState);

        

        foreach (Vector3Int cell in aliveCells)

        {

            for (int x = -1; x <= 1; x++)

            {

                for (int y = -1; y <= 1; y++)

                {

                    Vector3Int neighbor = cell + new Vector3Int(x, y, 0);

                    if (boardBounds.Contains(neighbor))

                    {

                        cellsToCheck.Add(neighbor);

                    }

                }

            }

        }



        foreach (Vector3Int cell in cellsToCheck)

        {

            if (!boardBounds.Contains(cell)) continue;

            

            int neighbors = CountNeighbors(cell);

            bool isAlive = IsAlive(cell);

            

            if (isAlive && (neighbors < 2 || neighbors > 3))

            {

                RemoveCell(cell, nextState);

            }

            else if (!isAlive && neighbors == 3)

            {

                AddCell(cell, nextState);

            }

            else

            {

                nextState.SetTile(cell, currentState.GetTile(cell));

            }

        }



        (nextState, currentState) = (currentState, nextState);

        nextState.ClearAllTiles();

    }



    private int CountNeighbors(Vector3Int cell)

    {

        int count = 0;

        for (int x = -1; x <= 1; x++)

        {

            for (int y = -1; y <= 1; y++)

            {

                if (x == 0 && y == 0) continue;

                

                Vector3Int neighbor = cell + new Vector3Int(x, y, 0);

                if (boardBounds.Contains(neighbor) && IsAlive(neighbor))

                {

                    count++;

                }

            }

        }

        return count;

    }



    private bool IsAlive(Vector3Int cell)

    {

        return boardBounds.Contains(cell) && currentState.GetTile(cell) == aliveTile;

    }



    private void Clear()

    {

        currentState.ClearAllTiles();

        nextState.ClearAllTiles();

    }



    private void Update()

    {

        HandleMouseInput();

    }



    private void HandleMouseInput()

    {

        if (isRunning) return;

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))

        {

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3Int cell = currentState.WorldToCell(mousePosition);

            

            Debug.Log($"Mouse: {Input.mousePosition}, World: {mousePosition}, Cell: {cell}, InBounds: {boardBounds.Contains(cell)}");

            

            if (boardBounds.Contains(cell))

            {

                if (Input.GetMouseButton(0)) // ЛКМ

            {

                if (!IsAlive(cell))

                {

                    AddCell(cell, currentState);

                    Debug.Log("Cell added");

                }

                }

                else if (Input.GetMouseButton(1)) // ПКМ

            {

                if (IsAlive(cell))

                {

                    RemoveCell(cell, currentState);

                    Debug.Log("Cell removed");

                }

                }

            }

        }

    }



    private void AddCell(Vector3Int cell, Tilemap state)

    {

        state.SetTile(cell, aliveTile);

        aliveCells.Add(cell);

    }



    private void RemoveCell(Vector3Int cell, Tilemap state)

    {

        state.SetTile(cell, deadTile);

        aliveCells.Remove(cell);

    }

}