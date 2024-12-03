namespace FSharpGodot

(* In each of these states, the int represents the current score. *)
type State =
    | Attract_Screen of int
    | Start_Button_Pressed of int
    | Play of int
    | Game_Over_Screen of int

