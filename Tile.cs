using System.Collections;
using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle,
    Breakable
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_board;

    public TileType tileType = TileType.Normal;

    SpriteRenderer m_spriteRenderer;

    public int breakableValue = 0;
    public Sprite[] breakableSprites;

    public Color normalColor;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        
    }
    
    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }
    
    private void OnMouseDown()
    {
        if(m_board != null)
        {
            m_board.ClickTile(this);
        }
    }
    
    private void OnMouseEnter()
    {
        if (m_board != null)
        {
            m_board.DragToTile(this);
        }
    }
    
    private void OnMouseUp()
    {
        if (m_board != null)
        {
            m_board.ReleaseTile(this);
        }
    }

    public void BreakTile()
    {
        if(tileType != TileType.Breakable)
        {
            return;
        }

        StartCoroutine(BreakTileRoutine());
    }

    IEnumerator BreakTileRoutine()
    {
        breakableValue = Mathf.Clamp((--breakableValue) , 0, breakableValue);

        if (breakableSprites[breakableValue] != null)
        {
            if (breakableValue <= 0)
            {
                tileType = TileType.Normal;

                m_spriteRenderer.color = normalColor;
            }

            m_spriteRenderer.sprite = breakableSprites[breakableValue];
        }

        yield return new WaitForSeconds(0.25f);
    }
}
