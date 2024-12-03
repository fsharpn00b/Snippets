namespace FSharpGodot

open Godot

type FSharpMain () as this =
    inherit Node ()

(*
https://docs.godotengine.org/en/stable/tutorials/scripting/resources.html
Nodes give you functionality: they draw sprites, 3D models, simulate physics, arrange user interfaces, etc. Resources are data containers. They don't do anything on their own: instead, nodes use the data contained in resources.

Anything Godot saves or loads from disk is a resource. Be it a scene (a .tscn or an .scn file), an image, a script... Here are some Resource examples:
Texture
Script
Mesh
Animation
AudioStream
Font
Translation

When the engine loads a resource from disk, it only loads it once. If a copy of that resource is already in memory, trying to load the resource again will return the same copy every time. As resources only contain data, there is no need to duplicate them.
(end)
*)
(* A scene is simply something that is animated, as in, a scene is played. *)
    let _enemy_scene = GD.Load<PackedScene> "res://enemy.tscn"

    let _state_lock = System.Object ()

(* TODO2 Ideally, the state would be immutable and included as a parameter for each event handler. *)
    let mutable _state = Attract_Screen 0

(* Transition from Attract_Screen state to Start_Button_Pressed state. *)
    member _.hud_start_game_handler () : unit =
        lock _state_lock (fun () ->
            match _state with
            | Attract_Screen _ ->
                let score = 0
                let hud = this.GetNode<FSharpHud>("HUD")
                do
(* Update the score. *)
                    hud.update_score score
(* Update the message. *)
(* Unlike FSharpHud.ShowGameOver (), we do not use async here. *)
                    hud.show_message "Get Ready!"
(* Set the player position. *)
                    this.GetNode<FSharpPlayer>("Player").Start (this.GetNode<Marker2D>("StartPosition").Position)
(* Start the start timer. *)
                    this.GetNode<Timer>("StartTimer").Start ()
(* Update the state. *)
                    _state <- Start_Button_Pressed score
// TODO2 Consider throwing on unexpected state where appropriate.
            | _ -> ()
        )

(* Transition from Start_Button_Pressed state to Play state. *)
    member _.start_timer_timeout_handler () : unit =
        lock _state_lock (fun () ->
            match _state with
            | Start_Button_Pressed score ->
                do
                    this.GetNode<Timer>("EnemyTimer").Start ()
                    this.GetNode<Timer>("ScoreTimer").Start ()
                    _state <- Play score
            | _ -> ()
        )

(* We add an enemy each time the enemy timer expires. *)
    member _.enemy_timer_timeout_handler () : unit =
(* Spawn an enemy and add it to the Main scene. *)
        do FSharpEnemy.spawn_enemy _enemy_scene (this.GetNode<PathFollow2D> "EnemyPath/EnemySpawnLocation") |> this.AddChild

    member _.score_timer_timeout_handler () : unit =
        lock _state_lock (fun () ->
(* If we are not in the Play state, ignore this event. *)
            match _state with
            | Play score_1 ->
                let score_2 = score_1 + 1
                do
                    this.GetNode<FSharpHud>("HUD").update_score score_2
                    _state <- Play score_2
            | _ -> ()
        )

(* Transition from Play state to Game_Over_Screen state. *)
    member _.player_hit_handler () : unit =
        lock _state_lock (fun () ->
            match _state with
            | Play score ->
                do
                    this.GetNode<FSharpHud>("HUD").show_game_over () |> ignore
                    this.GetNode<Timer>("EnemyTimer").Stop ()
                    this.GetNode<Timer>("ScoreTimer").Stop ()
                    _state <- Game_Over_Screen score
            | _ -> ()
        )

(* Transition from Game_Over_Screen state back to Attract_Screen state. *)
    member _.hud_show_attract_screen_handler () : unit =
        lock _state_lock (fun () ->
            match _state with 
            | Game_Over_Screen score -> do _state <- Attract_Screen score
            | _ -> ()
        )
