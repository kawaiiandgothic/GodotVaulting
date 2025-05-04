using Godot;
using System;
using System.Threading;

public partial class PlayerController : CharacterBody3D {
        [Export] public float speed = 3f;
        [Export] public float jumpVelocity = 4f;
        [Export] public float cameraSensitivity = 0.001f;
        public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

        private Node3D head;
        private Camera3D camera;

        // check if object is too low to vault
        private RayCast3D vaultCheckLow;
        private RayCast3D vaultCheckHigh;

        public override void _Input(InputEvent @event) {
                if (@event is InputEventMouseMotion m) {
                        // look
                        head.RotateY(-m.Relative.X * cameraSensitivity);
                        camera.RotateX(-m.Relative.Y * cameraSensitivity);

                        Vector3 cameraRotation = camera.Rotation;
                        cameraRotation.X = Mathf.Clamp(cameraRotation.X,
                                Mathf.DegToRad(-80f), Mathf.DegToRad(80f));

                        camera.Rotation = cameraRotation;
                }
        }

        public override void _PhysicsProcess(double delta) {
                Vector3 velocity = Velocity;

                if (!IsOnFloor())
                        velocity.Y -= gravity * (float)delta;

                if (Input.IsActionJustPressed("jump")) {
                        // vault
                        if (vaultCheckLow.IsColliding() && !vaultCheckHigh.IsColliding()) {
                                var collider = vaultCheckLow.GetCollider() as Node3D;
                                if (collider.Rotation != Vector3.Zero)
                                        goto jump;

                                GlobalPosition = vaultCheckLow.GetCollisionPoint() + new Vector3(0,
                                        collider.GlobalPosition.Z + 2,
                                        0);
                        }

                        jump: velocity.Y = jumpVelocity;
                }

                // move
                Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_front", "move_back");
                Vector3 direction = (head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
                if (direction != Vector3.Zero) {
                        velocity.X = direction.X * speed;
                        velocity.Z = direction.Z * speed;
                }
                else {
                        velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
                        velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
                }
                Velocity = velocity;
                MoveAndSlide();
        }

        public override void _Ready() {
                Input.MouseMode = Input.MouseModeEnum.Captured;

                head = GetNode<Node3D>("Head");
                camera = GetNode<Camera3D>("Head/Camera3D");
                camera.Fov = 60;

                vaultCheckLow = GetNode<RayCast3D>("Head/VaultCheckLow");
                vaultCheckHigh = GetNode<RayCast3D>("Head/VaultCheckHigh");
        }
}