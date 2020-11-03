using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class BoardDeadLock : MonoBehaviour
{
    List<GamePiece> GetRowOrColumnList(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool chackRow = true)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        List<GamePiece> pieceList = new List<GamePiece>();

        for(int i = 0; i < listLength; i++)
        {
            if(chackRow)
            {
                if(x + i < width && y < height && allPieces[x + i, y] != null)
                {
                    pieceList.Add(allPieces[x + i, y]);
                }
            }

            else
            {
                if(x < width && y + i < height && allPieces[x, y + i] != null)
                {
                    pieceList.Add(allPieces[x, y + i]);
                }
            }
        }

        return pieceList;
    }

    List<GamePiece> GetMinimumMatches (List<GamePiece> gamePieces, int minForMatch = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();

        var groups = gamePieces.GroupBy(n => n.matchValue);

        foreach (var grp in groups)
        {
            if(grp.Count() >= minForMatch && grp.Key != MatchValue.None)
            {
                matches = grp.ToList();
            }
        }

        return matches;
    }

    List<GamePiece> GetNeighbors(GamePiece[,] allPieces, int x, int y)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        List<GamePiece> neighbors = new List<GamePiece>();

        Vector2[] searchDirections = new Vector2[4]
        {
            new Vector2(-1f,0f),
            new Vector2(1f,0f),
            new Vector2(0f,1f),
            new Vector2(0f,-1f)
        };

        foreach (Vector2 dir in searchDirections)
        {
            if(x + (int)dir.x >= 0 && x + (int)dir.x < width && y + (int)dir.y >= 0 && y + (int)dir.y < height)
            {
                if(allPieces[x + (int)dir.x, y + (int)dir.y] != null)
                {
                    if(!neighbors.Contains(allPieces[x + (int)dir.x, y + (int)dir.y]))
                    {
                        neighbors.Add(allPieces[x + (int)dir.x, y + (int)dir.y]);
                    }
                }
            }
        }

        return neighbors;
    }

    bool HasMoveAt(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<GamePiece> pieces = GetRowOrColumnList(allPieces, x, y, listLength, checkRow);

        List<GamePiece> matches = GetMinimumMatches(pieces, listLength - 1);

        GamePiece unMatchedPiece = null;

        if(pieces != null && matches != null)
        {
            if(pieces.Count == listLength && matches.Count == listLength-1)
            {
                unMatchedPiece = pieces.Except(matches).FirstOrDefault();
            }

            if(unMatchedPiece != null)
            {
                List<GamePiece> neighbors = GetNeighbors(allPieces, unMatchedPiece.xIndex, unMatchedPiece.yIndex);

                neighbors = neighbors.Except(matches).ToList();

                neighbors = neighbors.FindAll(n => n.matchValue == matches[0].matchValue);

                matches = matches.Union(neighbors).ToList();
            }

            if(matches.Count  >= listLength)
            {
                string rowColStr = (checkRow) ? "row" : "column";

                Debug.Log("========Available Move===========");

                if(unMatchedPiece != null)
                {
                    Debug.Log("move " + matches[0].matchValue + " piece to " + unMatchedPiece.xIndex + "," + unMatchedPiece.yIndex + " to from matcing " + rowColStr);
                }

                return true;
            }
        }

        return false;
    }

    public bool IsDeadLocked(GamePiece[,] allPieces, int listlength = 3)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        bool isDeadLocked = true;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(HasMoveAt(allPieces,i,j,listlength,true) || HasMoveAt(allPieces,i,j,listlength,false))
                {
                    isDeadLocked = false;
                }
            }
        }

        if(isDeadLocked)
        {
            Debug.Log("==============BOARD DEADLOCKED===============");
        }

        return isDeadLocked;
    }
}
