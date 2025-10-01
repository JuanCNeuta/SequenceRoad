//124567
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    
    public static MazeGenerator Instance { get; private set; }

    [SerializeField]
    private MazeCell mazeCellPrefab;

    [SerializeField]
    private int mazeWidth;

    [SerializeField]
    private int mazeDepth;

    private MazeCell[,] mazeGrid;
    private float cellSize = 1f;

    //Para ubicar el objeto para anunciar trigger de recoger llaves
    public GameObject objectMaze;

    [Tooltip("Instancia actual del personaje; se asigna en Awake().")]
    public GameObject character;

    [SerializeField]
    private List<GameObject> collectibles;
    [Header("Configuración de Coleccionables")]
    [Tooltip("Cantidad de coleccionables a colocar en el nivel.")]
    [SerializeField] private int totalCollectiblesToPlace = 4;

    private List<GameObject> collectibleInstances = new List<GameObject>();

    [Header("Obstuculos manuales")]
    [SerializeField] private bool enableManualObstacles = false;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject keysPrefab;
    [SerializeField]
    private int seed = -1;  // Valor por defecto (-1 significa aleatorio)

    //Lista de llaves en el mapa
    private List<GameObject> keysInstances = new List<GameObject>();

    private Dictionary<GameObject, GameObject> collectibleToObstacleMap = new Dictionary<GameObject, GameObject>();
    private void Awake()
    {
        //Instancia del Maze generator
        Instance = this;

        // Instanciar el personaje antes de que otros scripts (ItemSlot) en Start lo busquen
        int indexJugador = PlayerPrefs.GetInt("JugadorIndex", 0);
        GameManagerMenu gm = GameManagerMenu.instance;

        if (gm == null)
        {
            Debug.LogError("GameManagerMenu.instance es null. No se puede instanciar personaje en Awake.");
            return;
        }

        if (gm.personajes == null || gm.personajes.Count == 0)
        {
            Debug.LogWarning("No hay personajes configurados en GameManagerMenu.");
            return;
        }

        // Ajustar rango y obtener prefab
        indexJugador = Mathf.Clamp(indexJugador, 0, gm.personajes.Count - 1);
        GameObject prefab = gm.personajes[indexJugador].personajeJugable;

        if (prefab == null)
        {
            Debug.LogError($"Prefab de personaje en �ndice {indexJugador} no asignado.");
            return;
        }

        // Instanciar y desactivar hasta posicionar
        character = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        character.name = prefab.name;
        character.SetActive(false);
    }

    void Start()
    {
        // Establecer la semilla si es distinta de -1
        if (seed != -1)
        {
            Random.InitState(seed);
            Debug.Log($"Generando laberinto con semilla: {seed}");
        }
        else
        {
            seed = Random.Range(0, int.MaxValue);
            Random.InitState(seed);
            Debug.Log($"Semilla generada aleatoriamente: {seed}");
        }

        mazeGrid = new MazeCell[mazeWidth, mazeDepth];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeDepth; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 1.9f, z * cellSize);
                mazeGrid[x, z] = Instantiate(mazeCellPrefab, position, Quaternion.identity);
            }
        }

        GenerateMaze(null, mazeGrid[0, 0]);

        // -- Posicionar y activar el personaje --
        if (character != null)
        {
            character.transform.position = new Vector3(0, 2, 0);
            character.transform.rotation = Quaternion.identity;
            character.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Character no instanciado en Awake().");
            
        }

        PlaceCollectibles();

        if (enableManualObstacles)
        {
            PlaceKeys(new Vector3(3.98f, 2.4f, 3.99f));
            PlaceKeys(new Vector3(5.008079f, 2.4f, 0.02f));
            PlaceManualObstacles();
        }

        // Llama a GenerateGrid() para dibujar la cuadr�cula en el piso
        GenerateGrid();
    }

private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        MazeCell nextCell;
        do
        {
            nextCell = GetNextUnvisitedCell(currentCell);
            if (nextCell != null)
            {
                GenerateMaze(currentCell, nextCell);
            }
        } while (nextCell != null);
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell);
        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        if (x + 1 < mazeWidth && !mazeGrid[x + 1, z].IsVisited)
            yield return mazeGrid[x + 1, z];

        if (x - 1 >= 0 && !mazeGrid[x - 1, z].IsVisited)
            yield return mazeGrid[x - 1, z];

        if (z + 1 < mazeDepth && !mazeGrid[x, z + 1].IsVisited)
            yield return mazeGrid[x, z + 1];

        if (z - 1 >= 0 && !mazeGrid[x, z - 1].IsVisited)
            yield return mazeGrid[x, z - 1];
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null) return;

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        if (previousCell.transform.position.z < currentCell.transform.position.z)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        if (previousCell.transform.position.z > currentCell.transform.position.z)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }

    private void PlaceCollectibles()
    {
        Random.InitState(seed);

        List<Vector3> validPositions = new List<Vector3>();
        Vector3 characterStartPosition = character.transform.position;
        Vector2 characterXZ = new Vector2(characterStartPosition.x, characterStartPosition.z);

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeDepth; z++)
            {
                MazeCell cell = mazeGrid[x, z];
                Vector3 cellPosition = cell.transform.position;
                Vector2 cellXZ = new Vector2(cellPosition.x, cellPosition.z);

                if (cell.HasWalls() && Vector2.Distance(cellXZ, characterXZ) > 0.5f)
                {
                    validPositions.Add(cellPosition);
                }
            }
        }

        HashSet<Vector3> usedPositions = new HashSet<Vector3>();
        usedPositions.Add(new Vector3(Mathf.Floor(characterStartPosition.x / cellSize) * cellSize, 0, Mathf.Floor(characterStartPosition.z / cellSize) * cellSize));

        int countToPlace = Mathf.Min(totalCollectiblesToPlace, collectibles.Count);

        // Elegir aleatoriamente sin repetir
        List<GameObject> selectedCollectibles = collectibles.OrderBy(_ => Random.value).Take(countToPlace).ToList();

        for (int i = 0; i < selectedCollectibles.Count; i++)
        {
            if (i >= collectibleInstances.Count)
            {
                GameObject collectibleInstance = Instantiate(selectedCollectibles[i], Vector3.zero, selectedCollectibles[i].transform.rotation);
                collectibleInstances.Add(collectibleInstance);
            }
            else
            {
                collectibleInstances[i].name = selectedCollectibles[i].name;
            }

            var availablePositions = validPositions.Where(pos => !usedPositions.Contains(pos)).ToList();

            if (availablePositions.Count == 0)
            {
                Debug.LogWarning("No hay suficientes posiciones válidas.");
                return;
            }

            int index = Random.Range(0, availablePositions.Count);
            Vector3 position = availablePositions[index];

            collectibleInstances[i].transform.position = position + Vector3.up * 0.9f;
            collectibleInstances[i].SetActive(true);

            // Animación
            CollectibleAnimation anim = collectibleInstances[i].GetComponent<CollectibleAnimation>();
            if (anim != null)
            {
                anim.StopAllCoroutines();
                anim.StartCoroutine("AnimateScale");
            }

            usedPositions.Add(position);

            CollectibleManager.Instance.RegisterCollectible(collectibleInstances[i]);
        }

        CollectibleManager.Instance?.InitializeCollectibles(selectedCollectibles.Count);
    }

    //Metodo para posicionar las llaves para desbloquear los obstaculos
    private void PlaceKeys(Vector3 keysPosition)
    {
        keysPrefab.SetActive(true);
        GameObject key = Instantiate(keysPrefab, keysPosition, Quaternion.identity);
        keysInstances.Add(key);
    }

    private void PlaceObject(Vector3 objectPosition, GameObject objectMaze)
    {
        GameObject mazeObstacule = Instantiate(objectMaze, objectPosition, Quaternion.identity);
        mazeObstacule.SetActive(true);
    }

    // Se pone los obstaculos en el mapa de manera manual y se los asocia al objeto que debera ser tomado y desaparecera el obstaculo
    private void PlaceManualObstacles()
    {
        obstaclePrefab.SetActive(true);
        // Obstáculo sin rotación
        LinkExistingCollectibleToObstacle(new Vector3(3.98f, 2.4f, 3.99f), new Vector3(4.704f, 2.4f, 2.336f));

        // Obstáculo con rotación de 90° en Y
        Quaternion rotationY90 = Quaternion.Euler(0, 130, 0);
        LinkExistingCollectibleToObstacle(new Vector3(5.008079f, 2.4f, 0.02f), new Vector3(2.37f, 2.4f, 5.32f), rotationY90);

        //Obstaculo trasnparente para trigger
        PlaceObject(new Vector3(1.0f, 2.78f, -0.079f), objectMaze);
    }

    // Metodo para asociar obst�culo a un coleccionable ya existente
    private void LinkExistingCollectibleToObstacle(Vector3 collectiblePosition, Vector3 obstaclePosition, Quaternion? rotation = null)
    {
        GameObject existingCollectible = (collectibleInstances.Concat(keysInstances))
            .FirstOrDefault(c => Vector3.Distance(c.transform.position, collectiblePosition) < 0.5f);

        if (existingCollectible == null)
        {
            Debug.LogWarning("No se encontró el coleccionable en la posición especificada.");
            return;
        }

        // Si se pasa una rotación, se úsa; si no, usa rotación por defecto (sin girar)
        Quaternion obstacleRotation = rotation ?? Quaternion.identity;

        GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, obstacleRotation);
        collectibleToObstacleMap[existingCollectible] = obstacle;
    }

    public bool ObstacleLinkedToCollectible(GameObject collectible, out GameObject obstacle)
    {
        return collectibleToObstacleMap.TryGetValue(collectible, out obstacle);
    }

    private void GenerateGrid()
    {
        // Crea un GameObject padre para organizar las l�neas de la cuadr�cula
        GameObject gridParent = new GameObject("GridLines");
        gridParent.transform.position = Vector3.zero;

        float offset = cellSize * 0.5f;

        float gridY = 2.14f;

        // L�neas verticales
        for (int x = 0; x <= mazeWidth; x++)
        {
            Vector3 start = new Vector3(x * cellSize - offset, gridY, -offset);
            Vector3 end = new Vector3(x * cellSize - offset, gridY, mazeDepth * cellSize - offset);
            CreateLine(start, end, gridParent.transform);
        }

        // L�neas horizontales
        for (int z = 0; z <= mazeDepth; z++)
        {
            Vector3 start = new Vector3(-offset, gridY, z * cellSize - offset);
            Vector3 end = new Vector3(mazeWidth * cellSize - offset, gridY, z * cellSize - offset);
            CreateLine(start, end, gridParent.transform);
        }

    }

    private void CreateLine(Vector3 start, Vector3 end, Transform parent)
    {
        // Crea un GameObject para la l�nea
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = parent;

        // A�ade el componente LineRenderer
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Configura el ancho de la l�nea
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        lr.material = new Material(Shader.Find("Sprites/Default"));

        // Define el color de la l�nea
        lr.startColor = Color.black;
        lr.endColor = Color.black;
    }

    public void ResetCollectibles()
    {
        // Limpia el estado del CollectibleManager
        CollectibleManager.Instance.ResetManager();

        foreach (GameObject collectible in collectibleInstances)
        {
            if (collectible != null)
                collectible.SetActive(false);
        }

        PlaceCollectibles(); // Vuelve a posicionar y registrar
    }

    public void ClearCollectibles()
    {
        foreach (GameObject collectible in collectibleInstances)
        {
            collectible.SetActive(false);
        }
    }

    public void SetSeed(int newSeed)
{
    seed = newSeed;
    PlayerPrefs.SetInt("MazeSeed", seed);
    PlayerPrefs.Save();
}

    public int GetTotalCollectibles()
    {
        return collectibles.Count;
    }
}
