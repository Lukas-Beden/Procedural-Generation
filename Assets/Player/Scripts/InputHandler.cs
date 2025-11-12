using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class InputHandler : MonoBehaviour
{
    private Vector2 _move = new();
    private Vector2 _look = new();
    private bool _jump;
    private bool _draw;
    private bool _drawReleasedThisFrame;
    private bool _drawPressedThisFrame;
    private float _changePaint;

    public Vector2 Move => _move;
    public Vector2 Look => _look;
    public bool Jump => _jump;
    public bool Draw => _draw;
    public bool DrawReleasedThisFrame => _drawReleasedThisFrame;
    public bool DrawPressedThisFrame => _drawPressedThisFrame;
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

    public void OnDraw(CallbackContext ctx)
    {
        if (ctx.started)
        {
            _draw = true;
            _drawPressedThisFrame = true;
        } 
        if (ctx.canceled)
        {
            _draw = false;
            _drawReleasedThisFrame = true;
        }
    }

    public void OnChangePaint(CallbackContext ctx)
    {
        if (!_draw)
            _changePaint = ctx.ReadValue<float>();
    }


    private void LateUpdate()
    {
        _drawPressedThisFrame = false;
        _drawReleasedThisFrame = false;
    }
}
