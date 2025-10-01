using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour
{
    public Animator animator;

    // Método para actualizar la animación según la dirección de movimiento
    public void MoveCharacter(Vector3 direction)
    {
        if (direction != Vector3.zero) // Si hay movimiento
        {
            transform.forward = direction; // Hace que el personaje mire hacia la dirección del movimiento

            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }

    public void JumpCharacter(float happyDuration = 2f)
    {
        StartCoroutine(HappyRoutine(happyDuration));
    }

    private IEnumerator HappyRoutine(float duration)
    {
        animator.SetBool("IsHappy", true);

        yield return new WaitForSeconds(duration);

        animator.SetBool("IsHappy", false);
    }

    // animación de victoria aleatoria
    public void PlayVictoryAnimation(float victoryDuration = 2f)
    {
        int randomIndex = Random.Range(1, 4); // Elige entre Victory1, Victory2, Victory3
        string victoryParam = "Victory" + randomIndex;

        StartCoroutine(VictoryRoutine(victoryParam, victoryDuration));
    }

    private IEnumerator VictoryRoutine(string paramName, float duration)
    {
        animator.SetBool(paramName, true);
        yield return new WaitForSeconds(duration);
        animator.SetBool(paramName, false);
    }

    public void WinLevelCharacter(float happyDuration = 2f)
    {
        StartCoroutine(WinLevelRoutine(happyDuration));
    }

    private IEnumerator WinLevelRoutine(float duration)
    {
        animator.SetBool("Victory1", true);

        yield return new WaitForSeconds(duration);

        animator.SetBool("Victory1", false);
    }

    public void PlayObstaculeAnimation(float obstaculeDuration = 2f)
    {
        StartCoroutine(ObstaculeRoutine(obstaculeDuration));
    }

    private IEnumerator ObstaculeRoutine(float duration)
    {
        animator.SetBool("Obstacule", true);

        yield return new WaitForSeconds(duration);

        animator.SetBool("Obstacule", false);
    }

    public void StopCharacter()
    {
        animator.SetBool("IsWalking", false);
    }
}
