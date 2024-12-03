namespace FSharpGodot

open Godot

type FSharpEnemy () =
    inherit RigidBody2D ()

(* 
https://docs.godotengine.org/en/stable/classes/class_node.html#class-node-private-method-ready
void _ready ( ) virtual

Called when the node is "ready", i.e. when both the node and its children have entered the scene tree. If the node has children, their _ready callbacks get triggered first, and the parent node will receive the ready notification afterwards.
...
Note: _ready may be called only once for each node....
(end)
*)
    override this._Ready () =
(* We do this here, not in spawn_enemy, because that method is static and cannot access this and therefore cannot call GetNode. *)
        let animatedSprite2D = this.GetNode<AnimatedSprite2D> "AnimatedSprite2D"
        let enemy_types = animatedSprite2D.SpriteFrames.GetAnimationNames ()
(* Get a random enemy type. *)
(* TODO2 Presumably, Play () is async, but we can't find that in the documentation. *)
        do animatedSprite2D.Play (enemy_types[System.Random().Next (0, enemy_types.Length)])
        ()

    static member spawn_enemy (enemy_scene : PackedScene) (enemy_spawn_location : PathFollow2D) : FSharpEnemy =
(* We instantiate FSharpEnemy in code, rather than in the configuration as we do for FSharpPlayer and FSharpHud, because:
1 We instantiate FSharpEnemy multiple times.
2 We instantiate FSharpEnemy in response to an event (enemy timer timeout).
 *)
        let enemy = enemy_scene.Instantiate<FSharpEnemy> ()

(* Choose a random location on Path2D. *)
        do enemy_spawn_location.ProgressRatio <- GD.Randf ()

(* Set the enemy's direction perpendicular to the path direction. *)
        let direction_1 = enemy_spawn_location.Rotation + Mathf.Pi / 2f

(* Position is a property of Node2D. *)
        do enemy.Position <- enemy_spawn_location.Position

(* Randomize the enemy direction. *)
        let from = (float) -Mathf.Pi / 4.0
        let to_ = (float) Mathf.Pi / 4.0
        let direction_2 = (float) direction_1 + GD.RandRange (from, to_)
        do enemy.Rotation <- (float32) direction_2

(* Randomize the enemy velocity. *)
        let velocity_1 = GD.RandRange (150, 250)
        let velocity_2 = Vector2 ((float32) velocity_1, 0f)
        do enemy.LinearVelocity <- velocity_2.Rotated ((float32) direction_2)

        enemy

    member this.visible_on_screen_notifier_2d_enemy_screen_exited_handler () : unit =
        do this.QueueFree ()
