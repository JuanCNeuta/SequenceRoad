using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField]
    private GameObject leftWall;

    [SerializeField]
    private GameObject rightWall;

    [SerializeField]
    private GameObject frontWall;

    [SerializeField] 
    private GameObject backWall;

    [SerializeField]
    private GameObject unvisitedWall;

    public bool IsVisited { get; private set; }

    public void Visit()
    {
        IsVisited = true;
        unvisitedWall.SetActive(false);
    }

    public void ClearLeftWall()
    {
        leftWall.SetActive(false);
    }

    public void ClearRightWall()
    {
        rightWall.SetActive(false);
    }

    public void ClearFrontWall()
    {
       frontWall.SetActive(false);
    }

    public void ClearBackWall()
    {
        backWall.SetActive(false);
    }

    public bool HasWalls()
    {
        // Revisa si hay al menos una pared activa
        bool hasActiveWall = leftWall.activeSelf || rightWall.activeSelf || frontWall.activeSelf || backWall.activeSelf;

        // Revisa si hay al menos una pared desactivada
        bool hasInactiveWall = !leftWall.activeSelf || !rightWall.activeSelf || !frontWall.activeSelf || !backWall.activeSelf;

        // Retorna true solo si hay al menos una pared activa y al menos una desactivada
        return hasActiveWall && hasInactiveWall;
    }


}
