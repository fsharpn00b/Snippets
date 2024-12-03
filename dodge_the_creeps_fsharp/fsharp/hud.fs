namespace FSharpGodot

open Godot

(* See
https://lenscas.github.io/posts/fsharp_and_godot_part2/
*)
type SignalAwaiter2 (wrapped: SignalAwaiter) =
    interface System.Runtime.CompilerServices.ICriticalNotifyCompletion with
        member this.OnCompleted(cont) = wrapped.OnCompleted(cont)
        member this.UnsafeOnCompleted(cont) = wrapped.OnCompleted(cont)

    member this.IsCompleted = wrapped.IsCompleted

    member this.GetResult() = wrapped.GetResult()

    member this.GetAwaiter() = this

type FSharpHud () =
    inherit CanvasLayer ()

(* We use this method to show the "Get Ready" and "Game Over" messages. *)
    member this.show_message (text : string) : unit =
        let message = this.GetNode<Label> "Message"
        do
            message.Text <- text
            message.Show ()
            this.GetNode<Timer>("MessageTimer").Start ()

(* TODO2 Explain why this is async/task, when the call to show_message () in FSharpMain.hud_start_game_handler () is not. *)
    member this.show_game_over () : System.Threading.Tasks.Task<unit> =
        task {
            this.show_message "Game Over"

            let message_timer = this.GetNode<Timer> "MessageTimer"
(* We cannot use do! here because SignalAwaiter has type parameter Variant array, not unit. See
https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/SignalAwaiter.cs
(end)
*)
            let! _ = SignalAwaiter2 (this.ToSignal(message_timer, Timer.SignalName.Timeout))

            let message = this.GetNode<Label> "Message"
            do
                message.Text <- "Dodge the Creeps!"
                message.Show ()

            let! _ = SignalAwaiter2 (this.ToSignal(this.GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout))

            this.EmitSignal ("ShowAttractScreen") |> ignore
            do this.GetNode<Button>("StartButton").Show ()
        }

    member this.update_score (score : int) : unit =
        do this.GetNode<Label>("ScoreLabel").Text <- score.ToString ()

    member this.start_button_pressed_handler () : unit =
        do
            this.GetNode<Button>("StartButton").Hide () |> ignore
            this.EmitSignal "StartGame" |> ignore

    member this.message_timer_timeout_handler () : unit =
        do this.GetNode<Label>("Message").Hide ()
