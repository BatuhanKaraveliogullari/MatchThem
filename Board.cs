using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoardDeadLock))]
[RequireComponent(typeof(BoardShuffler))]
public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public float swapTime = 0.5f;

    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject[] gamePiecePrefabs;

    public GameObject[] columnBombPrefabs;
    public GameObject[] rowBombPrefabs;
    public GameObject[] adjacentBombPrefabs;
    public GameObject colorBombPrefab;

    public int maxCollectibles = 3;
    public int collectibleCount = 0;
    [Range(0,1)]
    public float chanceForCollectible = 0.1f;
    public GameObject[] collectiblePrefabs;

    GameObject m_clickedTileBomb;
    GameObject m_targetTileBomb;

    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePiece;

    Tile m_clickedTile;
    Tile m_targetTile;

    bool m_playerInputEnabled = true;

    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;
 

    ParticalManager m_particalManager;

    public int fillYOffset = 10;
    public float fillMoveTime = 0.5f;

    int m_scoreMultiplier = 0;

    public bool isRefilling = false;

    BoardDeadLock m_boardDeadLock;

    BoardShuffler m_boardShuffler;

    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;

        public int x;
        public int y;
        public int z;
    }

    void Start()
    {
        m_allTiles = new Tile[width, height];

        m_allGamePiece = new GamePiece[width, height];

        m_particalManager = GameObject.FindWithTag("ParticalManager").GetComponent<ParticalManager>();

        m_boardDeadLock = GetComponent<BoardDeadLock>();

        m_boardShuffler = GetComponent<BoardShuffler>();
    }

    public void SetupBoard()
    {
        SetupTiles();

        SetupGamePieces();

        List<GamePiece> startingCollectibles = FindAllCollectibles();

        collectibleCount = startingCollectibles.Count;

        SetupCamera();

        FillBoard(fillYOffset, fillMoveTime);
    }

    private void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if(prefab != null && IsWithinBounds(x,y))
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;

            tile.name = "Tile (" + x + "," + y + ")";

            m_allTiles[x, y] = tile.GetComponent<Tile>();

            tile.transform.parent = transform;

            m_allTiles[x, y].Init(x, y, this);
        }
    }

    void MakeGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (prefab != null && IsWithinBounds(x,y))
        {
            prefab.GetComponent<GamePiece>().Init(this);

            PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);

                prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
            }

            prefab.transform.parent = transform;
        }
    }

    GameObject MakeBomb(GameObject prefab, int x, int y)
    {
        if(prefab != null && IsWithinBounds(x,y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;

            bomb.GetComponent<Bomb>().Init(this);

            bomb.GetComponent<Bomb>().SetCoord(x, y);

            bomb.transform.parent = transform;

            return bomb;
        }

        return null;
    }

    public void MakeColorBombBooster(int x, int y)
    {
        if(IsWithinBounds(x,y))
        {
            GamePiece pieceToReplace = m_allGamePiece[x, y];

            if(pieceToReplace != null)
            {
                ClearPieceAt(x, y);

                GameObject bombObject = MakeBomb(colorBombPrefab, x, y);

                ActiveteBomb(bombObject);
            }
        }
    }

    void SetupTiles()
    {
        foreach (StartingObject sTile in startingTiles)
        {
            if(sTile != null)
            {
                MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(m_allTiles[i,j] == null)
                {
                    MakeTile(tileNormalPrefab, i, j);
                }
            }
        }
    }

    void SetupGamePieces()
    {
        foreach (StartingObject sPiece in startingGamePieces)
        {
            if(sPiece != null)
            {
                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;

                MakeGamePiece(piece, sPiece.x, sPiece.y, fillYOffset, fillMoveTime);
            }
        }
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float verticalSize = (float)height / 2f + (float)borderSize;

        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize; /******************************Mükemmel**********************************/

        /* yukarıdaki ifade aşağıdaki ifadenin aynı anlamına geliyor.
         * if(verticalSize > horizontalSize)
         * {
         *      Camera.main.orthographicSize = verticalSize;
         * }
         * else
         * {
         *      Camera.main.orthographicSize = horizontalSize;
         * }
         */
    }

    GameObject GetRandomObject(GameObject[] objectArray)
    {
        int randomIdx = Random.Range(0, objectArray.Length);

        if(objectArray[randomIdx] == null)
        {
            Debug.LogWarning("BOARD: GetRandomObject at index" + randomIdx + "does not contain a valid GameObject!");
        }

        return objectArray[randomIdx];
    }

    GameObject GetRandomGamePiece()
    {
        return GetRandomObject(gamePiecePrefabs);
    }

    GameObject GetRandomCollectible()
    {
        return GetRandomObject(collectiblePrefabs);
    }
    
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: invalid GamePiece");

            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);

        gamePiece.transform.rotation = Quaternion.identity;

        if (IsWithinBounds(x, y))
        {
            m_allGamePiece[x, y] = gamePiece;
        }

        gamePiece.SetCoord(x, y);
    }
    
    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if(IsWithinBounds(x,y))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);

            return randomPiece.GetComponent<GamePiece>();
        }

        return null;
    }

    GamePiece FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if(IsWithinBounds(x,y))
        {
            GameObject randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity) as GameObject;

            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);

            return randomPiece.GetComponent<GamePiece>();
        }

        return null;
    }

    void FillBoardFromList(List<GamePiece> gamePieces)
    {
        Queue<GamePiece> unUsedPieces = new Queue<GamePiece>(gamePieces);

        int maxIterations = 100;

        int iterations = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(m_allGamePiece[i,j] == null && m_allTiles[i,j].tileType != TileType.Obstacle)
                {
                    m_allGamePiece[i, j] = unUsedPieces.Dequeue();

                    iterations = 0;

                    while(HasMatchesOnFill(i,j))
                    {
                        unUsedPieces.Enqueue(m_allGamePiece[i, j]);

                        m_allGamePiece[i, j] = unUsedPieces.Dequeue();

                        iterations++;

                        if(iterations >= maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }

    }

    void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxInterations = 100;
        int iteration = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(m_allGamePiece[i,j] == null && m_allTiles[i,j].tileType != TileType.Obstacle)
                {
                    //GamePiece piece = null;
                    
                    if(j == height - 1 && CanAddCollectible())
                    {
                        FillRandomCollectibleAt(i, j, falseYOffset, moveTime);

                        collectibleCount++;
                    }

                    else
                    {
                        FillRandomGamePieceAt(i, j, falseYOffset, moveTime);

                        iteration = 0;

                        while (HasMatchesOnFill(i, j))
                        {
                            ClearPieceAt(i, j);

                            FillRandomGamePieceAt(i, j, falseYOffset, moveTime);

                            iteration++;

                            if (iteration >= maxInterations)
                            {
                                Debug.Log("break ===================");

                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    bool HasMatchesOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if(leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        
        if(downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }

    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;

            Debug.Log("Clicked tile : " + tile.name);
        }
    }
    
    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }
    
    public void ReleaseTile(Tile tile)
    {
        if (m_targetTile != null && m_clickedTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }
    
    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchesTileRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchesTileRoutine(Tile clickedTile, Tile targetTile) 
    {
        if(m_playerInputEnabled && !GameManager.Insatance.IsGameOver)
        {  
            GamePiece clickedPiece = m_allGamePiece[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePiece[targetTile.xIndex, targetTile.yIndex];

            if (targetPiece != null && clickedPiece != null)
            {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

                List<GamePiece> colorMatches = ProcessColorBombs(clickedPiece, targetPiece);

                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                }

                else
                {
                    yield return new WaitForSeconds(swapTime);

                    #region drop bomb
                    Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);

                    m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swipeDirection, clickedPieceMatches);

                    m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swipeDirection, targetPieceMatches);

                    if (m_clickedTileBomb != null && targetPiece != null)
                    {
                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();

                        if (!IsColorBomb(clickedBombPiece))
                        {
                            clickedBombPiece.ChangeColor(targetPiece);
                        }
                    }

                    if (m_targetTileBomb != null && clickedPiece != null)
                    {
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();

                        if (!IsColorBomb(targetBombPiece))
                        {
                            targetBombPiece.ChangeColor(clickedPiece);
                        }
                    }
                    #endregion

                    List<GamePiece> pieceToClear = clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList();

                    yield return StartCoroutine(ClearAndRefillBoardRoutine(pieceToClear));


                    if (GameManager.Insatance != null)
                    {
                        GameManager.Insatance.UpdateMoves();
                    }
                }
            }
        }
    }

    private List<GamePiece> ProcessColorBombs(GamePiece clickedPiece, GamePiece targetPiece, bool clearNonBlockers = false)
    {
        List<GamePiece> colorMatches = new List<GamePiece>();

        GamePiece colorBombPiece = null;
        GamePiece otherPiece = null;

        if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
        {
            colorBombPiece = clickedPiece;
            otherPiece = targetPiece;
        }
        else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
        {
            colorBombPiece = targetPiece;
            otherPiece = clickedPiece;
        }
        else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
        {
            foreach (GamePiece piece in m_allGamePiece)
            {
                if (!colorMatches.Contains(piece))
                {
                    colorMatches.Add(piece);
                }
            }
        }

        if(colorBombPiece != null)
        {
            colorBombPiece.matchValue = otherPiece.matchValue;

            colorMatches = FindAllMatchValue(otherPiece.matchValue);
        }

        if(!clearNonBlockers)
        {
            List<GamePiece> collectedATBottom = FindAllCollectibles(true);

            if(collectedATBottom.Contains(otherPiece))
            {
                return new List<GamePiece>();
            }

            else
            {
                foreach (GamePiece piece in collectedATBottom)
                {
                    colorMatches.Remove(piece);
                }
            }
        }

        return colorMatches;
    }

    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        else if (start.xIndex == end.xIndex && Mathf.Abs(start.yIndex - end.yIndex) == 1)
        {
            return true;
        }

        return false;
    }
    
    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePiece[startX, startY];
        }
        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height; // if clause

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = m_allGamePiece[nextX, nextY];

            if(nextPiece == null)
            {
                break;
            }

            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
                {
                    matches.Add(nextPiece);
                }

                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }

        return null;
    }
    
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }
    
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }
    
    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
         List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
         List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);
        
         if (horizMatches == null)
         {
             horizMatches = new List<GamePiece>();
         }
        
         if (vertMatches == null)
         {
             vertMatches = new List<GamePiece>();
         }
        
         var combineMatches = horizMatches.Union(vertMatches).ToList();
        
         return combineMatches;
    }

    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
            }
        }

        return matches;
    }

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);

                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
    }
    
    void HighlightTileOff(int x, int y)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    void HighlightTileOn(int x, int y, Color col)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = col;
        }
    }

    void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);

        var combineMatches = FindMatchesAt(x, y);

        if (combineMatches.Count > 0)
        {
            foreach (GamePiece piece in combineMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    void HighlightPiece(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePiece[x, y];

        if(pieceToClear != null)
        {
            m_allGamePiece[x, y] = null;

            Destroy(pieceToClear.gameObject);
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);

                if(m_particalManager != null)
                {
                    m_particalManager.ClearPieceFXAt(i, j);
                }
            }
        }
    }

    void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);

                int bonus = 0;

                if(gamePieces.Count >= 4)
                {
                    bonus = 10;
                }

                if(GameManager.Insatance != null)
                {
                    GameManager.Insatance.ScorePoints(piece, m_scoreMultiplier, bonus);

                    TimeBonus timeBonus = piece.GetComponent<TimeBonus>();

                    if(timeBonus != null)
                    {
                        GameManager.Insatance.AddTime(timeBonus.bonusValue);

                        Debug.Log("BOARD Adding time bonus from " + piece.name + " of " + timeBonus.bonusValue);
                    }

                    GameManager.Insatance.UpdateCollectionGoals(piece);
                }

                if (m_particalManager != null)
                {
                    if(bombedPieces != null)
                    {
                        m_particalManager.BombFXAt(piece.xIndex, piece.yIndex);
                    }

                    else
                    {
                        m_particalManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                    }
                }
            }
        }
    }

    void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x, y];

        if(tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            if(m_particalManager != null)
            {
                m_particalManager.BreakTileFXAt(tileToBreak.breakableValue, x, y, 0);
            }

            tileToBreak.BreakTile();
        }
    }

    void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    List<GamePiece> CollapseColumn (int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height-1; i++)
        {
            if(m_allGamePiece[column,i] == null && m_allTiles[column,i].tileType != TileType.Obstacle)
            {
                for (int j =i + 1 ; j < height; j++)
                {
                    if(m_allGamePiece[column,j] != null)
                    {
                        m_allGamePiece[column, j].Move(column, i, collapseTime*(j-i));

                        m_allGamePiece[column, i] = m_allGamePiece[column, j];

                        m_allGamePiece[column, i].SetCoord(column, i);

                        if(!movingPieces.Contains(m_allGamePiece[column,i]))
                        {
                            movingPieces.Add(m_allGamePiece[column, i]);
                        }

                        m_allGamePiece[column, j] = null;

                        break;
                    }
                }
            }
        }

        return movingPieces;
    }

    List<GamePiece> CollapseColumn (List<GamePiece> gamepiece)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamepiece);

        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<int> columnToCollapse)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        foreach (int column in columnToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                if (!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }
        }

        return columns;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    public void ClearAndRefillBoard(int x, int y)
    {
        if(IsWithinBounds(x,y))
        {
            GamePiece pieceToClear = m_allGamePiece[x, y];

            List<GamePiece> listOfOne = new List<GamePiece>();

            listOfOne.Add(pieceToClear);

            ClearAndRefillBoard(listOfOne);
        }
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        m_playerInputEnabled = false;

        isRefilling = true;

        List<GamePiece> matches = gamePieces;

        m_scoreMultiplier = 0;

        do
        {
            m_scoreMultiplier++;

            yield return StartCoroutine(ClearAndCollapseRoutine(matches));

            yield return null;

            yield return StartCoroutine(RefillRoutine());

            matches = FindAllMatches();

            yield return new WaitForSeconds(0.2f);
        }
        while (matches.Count != 0);

        if (m_boardDeadLock.IsDeadLocked(m_allGamePiece, 3))
        {
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(ShuffleBoardRoutine());

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(RefillRoutine());
        }

        m_playerInputEnabled = true;

        isRefilling = false;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset,fillMoveTime);

        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        yield return new WaitForSeconds(0.2f);

        bool isFinished = false;

        while(!isFinished)
        {
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();


            List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);
            
            List<GamePiece> allCollectibles = FindAllCollectibles();
            List<GamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();
            collectibleCount -= collectedPieces.Count;

            gamePieces = gamePieces.Union(collectedPieces).ToList();

            List<int> columnToCollapse = GetColumns(gamePieces);

            ClearPieceAt(gamePieces, bombedPieces);

            BreakTileAt(gamePieces);

            if(m_clickedTileBomb != null)
            {
                ActiveteBomb(m_clickedTileBomb);

                m_clickedTileBomb = null;
            }
            
            if(m_targetTileBomb != null)
            {
                ActiveteBomb(m_targetTileBomb);

                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(columnToCollapse);

            while(!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchesAt(gamePieces);

            collectedPieces = FindCollectiblesAt(0, true);

            matches = matches.Union(collectedPieces).ToList();

            if(matches.Count == 0)
            {
                isFinished = true;

                break;
            }

            else
            {
                m_scoreMultiplier++;

                if(SoundManager.Insatance != null)
                {
                    SoundManager.Insatance.PlayBonusSound();
                }

                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }

        yield return null;
    }

    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                if(piece.transform.position.y - (float) piece.yIndex > 0.001f)
                {
                    return false;
                }

                if (piece.transform.position.x - (float)piece.xIndex > 0.001f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if(m_allGamePiece[i,row] != null)
            {
                gamePieces.Add(m_allGamePiece[i, row]);
            }
        }

        return gamePieces;
    }

    List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            if (m_allGamePiece[column,i] != null)
            {
                gamePieces.Add(m_allGamePiece[column,i]);
            }
        }

        return gamePieces;
    }

    List<GamePiece> GetAdjacentPieces (int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x - offset ; i <= x + offset; i++)
        {
            if(i >= 0 )
            {
                for (int j = y - offset; j <= y + offset; j++)
                {
                    if(j >= 0)
                    {
                        if (IsWithinBounds(x, y))
                        {
                            gamePieces.Add(m_allGamePiece[i, j]);
                        }
                    }
                }
            }
        }

        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();

                Bomb bomb = piece.GetComponent<Bomb>();

                if(bomb != null)
                {
                    switch(bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex,bomb.yIndex,1);
                            break;
                    }
                }

                allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();

                allPiecesToClear = RemoveCollectibles(allPiecesToClear);
            }
        }

        return allPiecesToClear;
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1; 
        int yStart = -1;

        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                if(xStart == -1 || yStart ==-1)
                {
                    xStart = piece.xIndex;

                    yStart = piece.yIndex;

                    continue;
                }

                if(piece.xIndex != xStart && piece.yIndex == yStart)
                {
                    horizontal = true;
                }

                if(piece.xIndex == xStart && piece.yIndex != yStart)
                {
                    vertical = true;
                }
            }
        }

        return (horizontal && vertical);
    }

    GameObject DropBomb (int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        GameObject bomb = null;

        MatchValue matchValue = MatchValue.None;

        if(gamePieces != null)
        {
            matchValue = FindMatchValue(gamePieces);
        }

        if (gamePieces.Count >= 5 && matchValue != MatchValue.None)
        {
            if (IsCornerMatch(gamePieces))
            {
                GameObject adjacentBomb = FindGamePieceByMatchValue(adjacentBombPrefabs, matchValue);

                if (adjacentBomb != null)
                {
                    bomb = MakeBomb(adjacentBomb, x, y);
                }
            }
            else
            {
                if (colorBombPrefab != null)
                {
                    bomb = MakeBomb(colorBombPrefab, x, y);
                }
            }
        }
        else if (gamePieces.Count == 4 && matchValue != MatchValue.None)
        {
            if (swapDirection.x != 0)
            {
                GameObject rowBomb = FindGamePieceByMatchValue(rowBombPrefabs, matchValue);

                if (rowBomb != null)
                {
                    bomb = MakeBomb(rowBomb, x, y);
                }
            }

            else
            {
                GameObject columnBomb = FindGamePieceByMatchValue(columnBombPrefabs, matchValue);

                if (columnBomb != null)
                {
                    bomb = MakeBomb(columnBomb, x, y);
                }
            }
        }
        
        return bomb;
    }

    void ActiveteBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if(IsWithinBounds(x,y))
        {
            m_allGamePiece[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    List<GamePiece> FindAllMatchValue(MatchValue mValue)
    {
        List<GamePiece> foundPieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(m_allGamePiece[i,j] != null)
                {
                    if(m_allGamePiece[i,j].matchValue == mValue)
                    {
                        foundPieces.Add(m_allGamePiece[i, j]);
                    }
                }
            }
        }

        return foundPieces;
    }

    bool IsColorBomb(GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if(bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }

        return false;
    }

    List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if(m_allGamePiece[i,row] != null)
            {
                Collectible collectibleComponent = m_allGamePiece[i, row].GetComponent<Collectible>();

                if(collectibleComponent != null)
                {
                    if(!clearedAtBottomOnly || (clearedAtBottomOnly && collectibleComponent.clearedAtBottom))
                    {
                        foundCollectibles.Add(m_allGamePiece[i, row]);
                    }
                }
            }
        }

        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles(bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i, clearedAtBottomOnly);

            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }

        return foundCollectibles;
    }

    bool CanAddCollectible()
    {
        return (Random.Range(0f, 1f) <= chanceForCollectible && collectiblePrefabs.Length > 0 && collectibleCount < maxCollectibles);
    }

    List<GamePiece> RemoveCollectibles(List<GamePiece> bombedPieces)
    {
        List<GamePiece> collectiblePieces = FindAllCollectibles();

        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach (GamePiece piece in collectiblePieces)
        {
            Collectible collectibleComponent = piece.GetComponent<Collectible>();

            if(collectibleComponent != null)
            {
                if(!collectibleComponent.clearedByBomb)
                {
                    piecesToRemove.Add(piece);
                }
            }
        }

        return bombedPieces.Except(piecesToRemove).ToList();
    }

    MatchValue FindMatchValue(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                return piece.matchValue;
            }
        }

        return MatchValue.None;
    }

    GameObject FindGamePieceByMatchValue(GameObject[] gamePiecePrefabs, MatchValue matchValue)
    {
        if(matchValue == MatchValue.None)
        {
            return null;
        }

        foreach (GameObject go in gamePiecePrefabs)
        {
            GamePiece piece = go.GetComponent<GamePiece>();

            if(piece != null)
            {
                if(piece.matchValue == matchValue)
                {
                    return go;
                }
            }
        }

        return null;
    }

    public void TestDeadLock()
    {
        if(m_boardDeadLock != null)
        {
            m_boardDeadLock.IsDeadLocked(m_allGamePiece, 3);
        }
    }

    public void ShuffleBoard()
    {
        if(m_playerInputEnabled)
        {
            StartCoroutine(ShuffleBoardRoutine());
        }
    }

    IEnumerator ShuffleBoardRoutine()
    {
        List<GamePiece> allPieces = new List<GamePiece>();

        foreach (GamePiece piece in m_allGamePiece)
        {
            allPieces.Add(piece);
        }

        while(!IsCollapsed(allPieces))
        {
            yield return null;
        }

        List<GamePiece> normalPieces = m_boardShuffler.RemoveNormalPieces(m_allGamePiece);

        m_boardShuffler.ShuffleList(normalPieces);

        FillBoardFromList(normalPieces);

        m_boardShuffler.MovePieces(m_allGamePiece, swapTime);

        List<GamePiece> matches = FindAllMatches();

        StartCoroutine(ClearAndRefillBoardRoutine(matches));
    }
}
 