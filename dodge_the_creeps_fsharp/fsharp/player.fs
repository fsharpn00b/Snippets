namespace FSharpGodot

open Godot

type FSharpPlayer () =
    inherit Area2D ()

(* The size of the game window. *)
    let mutable _screen_size : Vector2 option = None

(* How fast the player moves in pixels per second. *)
    let _speed = 400f

    override this._Ready () =
        do
            _screen_size <- this.GetViewportRect().Size |> Some
            this.Hide ()
        ()

    override this._Process (delta) =

(* The player's movement vector. *)
        let mutable velocity = Vector2.Zero
        if Input.IsActionPressed("move_right") then
            do velocity.X <- velocity.X + 1f
        if Input.IsActionPressed("move_left") then
            do velocity.X <- velocity.X - 1f
        if Input.IsActionPressed("move_down") then
            do velocity.Y <- velocity.Y + 1f
        if Input.IsActionPressed("move_up") then
            do velocity.Y <- velocity.Y - 1f

        let animatedSprite2D = this.GetNode<AnimatedSprite2D> "AnimatedSprite2D"

        if velocity.Length () > 0f then
            do
                velocity <- velocity.Normalized () * _speed;
                animatedSprite2D.Play ()
        else
            do animatedSprite2D.Stop ()

(* Position is a property of Node2D. *)
        do this.Position <- this.Position + (velocity * (float32) delta)
        do this.Position <- Vector2 (
            x = Mathf.Clamp (this.Position.X, 0f, _screen_size.Value.X),
            y = Mathf.Clamp (this.Position.Y, 0f, _screen_size.Value.Y)
        )
        
        if velocity.X <> 0f then
            do
                animatedSprite2D.Animation <- "walk"
                animatedSprite2D.FlipV <- false
                animatedSprite2D.FlipH <- velocity.X < 0f
        elif velocity.Y <> 0f then
            do
                animatedSprite2D.Animation <- "up"
                animatedSprite2D.FlipV <- velocity.Y > 0f
        ()

(* We do not check to see what entered the body. We assume it must be an enemy. *)
    member this.body_entered_handler _ : unit =
        do
            this.Hide (); // Player disappears after being hit.
            this.EmitSignal ("Hit") |> ignore
(* Disabling the area's collision shape can cause an error if it happens in the middle of the engine's collision processing. Using set_deferred() tells Godot to wait to disable the shape until it's safe to do so.
This must be deferred because we can't change physics properties on a physics callback.
*)
            this.GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

(* We do this here because _ready () is only called once for each node. *)
    member this.Start (position : Vector2) : unit =
        do
            this.Position <- position
            this.Show ()
            this.GetNode<CollisionShape2D>("CollisionShape2D").Disabled <- false
