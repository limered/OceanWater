using Godot;

namespace OceanWater;

public partial class CameraController : Camera3D
{
	private record CameraState
	{
		public float Yaw;
		public float Pitch;
		public float Roll;
		public Vector3 Position;

		public void FromTransform(Transform3D t)
		{
			Pitch = t.Basis.GetEuler().X;
			Yaw = t.Basis.GetEuler().Y;
			Roll = t.Basis.GetEuler().Z;
			Position = t.Origin;
		}

		public void Translate(Vector3 translation)
		{
			Position += Quaternion.FromEuler(new Vector3(Pitch, Yaw, Roll)) * translation;
		}
		
		public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
		{
			Pitch = Mathf.LerpAngle(Pitch, target.Pitch, rotationLerpPct);
			Yaw = Mathf.LerpAngle(Yaw, target.Yaw, rotationLerpPct);
			Roll = Mathf.LerpAngle(Roll, target.Roll, rotationLerpPct);
			
			Position = Position.Lerp(target.Position, positionLerpPct);
		}
		
		public Transform3D UpdateTransform(Transform3D t)
		{
			t.Basis = Basis.Identity
				.Rotated(Vector3.Right, Pitch)
				.Rotated(Vector3.Up, Yaw)
				.Rotated(Vector3.Forward, Roll);
			t.Origin = Position;
			return t;
		}
	}
	
	private readonly CameraState _targetCameraState = new();
	private readonly CameraState _interpolatingCameraState = new();

	[Export]
	public float Boost { get; set; } = 3.5f;
	[Export]
	public float PositionLerpTime { get; set; } = 0.2f;
	[Export]
	public float RotationLerpTime { get; set; } = 0.1f;
	
	public override void _Ready()
	{
		_targetCameraState.FromTransform(Transform);
		_interpolatingCameraState.FromTransform(Transform);
	}

	public override void _Process(double delta)
	{
		var translation = InputTranslation();
		_targetCameraState.Translate(translation);
		
		var positionLerpPct = 1f -  Mathf.Exp((Mathf.Log(1f - 0.99f) / PositionLerpTime) * delta);
		var rotationLerpPct = 1f -  Mathf.Exp((Mathf.Log(1f - 0.99f) / RotationLerpTime) * delta);
		_interpolatingCameraState.LerpTowards(_targetCameraState, (float)positionLerpPct, (float)rotationLerpPct);
		
		Transform = _interpolatingCameraState.UpdateTransform(Transform);
	}

	private Vector3 InputTranslation()
	{
		var translation = new Vector3();
		if(Input.IsActionPressed("Forward"))
			translation += Vector3.Forward;
		if(Input.IsActionPressed("Back"))
			translation += Vector3.Back;
		if(Input.IsActionPressed("Left"))
			translation += Vector3.Left;
		if(Input.IsActionPressed("Right"))
			translation += Vector3.Right;
		if(Input.IsActionPressed("Up"))
			translation += Vector3.Up;
		if(Input.IsActionPressed("Down"))
			translation += Vector3.Down;
		return translation;
	}
}