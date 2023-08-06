using Godot;

namespace OceanWater;

public partial class CameraController : Camera3D
{
    private readonly CameraState _interpolatingCameraState = new();

    private readonly CameraState _targetCameraState = new();
    private Vector2 _lastMousePosition;
    private bool _mouseActive;

    [Export] public float Boost { get; set; } = 3.5f;

    [Export] public float PositionLerpTime { get; set; } = 0.2f;

    [Export] public float RotationLerpTime { get; set; } = 0.1f;

    public override void _Ready()
    {
        _targetCameraState.FromTransform(Transform);
        _interpolatingCameraState.FromTransform(Transform);
    }


    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }:
                _mouseActive = true;
                _lastMousePosition = GetViewport().GetMousePosition();
                break;
            case InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left }:
                _mouseActive = false;
                break;
        }
    }

    public override void _Process(double delta)
    {
        ProcessTranslation();
        ProcessRotation();

        InterpolatePosition(delta);

        Transform = _interpolatingCameraState.UpdateTransform(Transform);
    }

    private void InterpolatePosition(double delta)
    {
        var positionLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / PositionLerpTime * delta);
        var rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / RotationLerpTime * delta);
        _interpolatingCameraState.LerpTowards(_targetCameraState, (float)positionLerpPct, (float)rotationLerpPct);
    }

    private void ProcessRotation()
    {
        if (!_mouseActive) return;

        var current = GetViewport().GetMousePosition();
        _targetCameraState.Rotate(_lastMousePosition - current);
        _lastMousePosition = current;
    }

    private void ProcessTranslation()
    {
        var translation = new Vector3();
        if (Input.IsActionPressed("Forward"))
            translation += Vector3.Forward;
        if (Input.IsActionPressed("Back"))
            translation += Vector3.Back;
        if (Input.IsActionPressed("Left"))
            translation += Vector3.Left;
        if (Input.IsActionPressed("Right"))
            translation += Vector3.Right;
        if (Input.IsActionPressed("Up"))
            translation += Vector3.Up;
        if (Input.IsActionPressed("Down"))
            translation += Vector3.Down;
        _targetCameraState.Translate(translation);
    }

    private record CameraState
    {
        private float _pitch;
        private Vector3 _position;
        private float _roll;
        private float _yaw;

        public void FromTransform(Transform3D t)
        {
            _pitch = t.Basis.GetEuler().X;
            _yaw = t.Basis.GetEuler().Y;
            _roll = t.Basis.GetEuler().Z;
            _position = t.Origin;
        }

        public void Translate(Vector3 translation)
        {
            _position += Quaternion.FromEuler(new Vector3(_pitch, _yaw, _roll)).Normalized() * translation;
        }

        public void Rotate(Vector2 rotation)
        {
            _yaw += rotation.X * 0.01f;
            _pitch += rotation.Y * 0.01f;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            _pitch = Mathf.LerpAngle(_pitch, target._pitch, rotationLerpPct);
            _yaw = Mathf.LerpAngle(_yaw, target._yaw, rotationLerpPct);
            _roll = Mathf.LerpAngle(_roll, target._roll, rotationLerpPct);

            _position = _position.Lerp(target._position, positionLerpPct);
        }

        public Transform3D UpdateTransform(Transform3D t)
        {
            t.Basis = Basis.Identity
                .Rotated(Vector3.Right, _pitch)
                .Rotated(Vector3.Up, _yaw)
                .Rotated(Vector3.Forward, _roll);
            t.Origin = _position;
            return t;
        }
    }
}