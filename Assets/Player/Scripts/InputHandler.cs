using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class InputHandler : MonoBehaviour
{
    private Vector2 _move = new();
    private Vector2 _look = new();
    private bool _jump;
    private float _changePaint;

    public Vector2 Move => _move;
    public Vector2 Look => _look;
    public bool Jump => _jump;
    public float ChangePaint => _changePaint;

    public void OnMove(CallbackContext ctx)
    {
        _move = ctx.ReadValue<Vector2>();
    }

    public void OnLook(CallbackContext ctx)
    {
        _look = ctx.ReadValue<Vector2>();
    }

    public void OnJump(CallbackContext ctx)
    {
        if (ctx.started)
        {
            _jump = true;
        } 
        if (ctx.canceled)
        {
            _jump = false;
        }
    }

    public void OnChangePaint(CallbackContext ctx)
    {
        _changePaint = ctx.ReadValue<float>();
    }
}
