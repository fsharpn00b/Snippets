open System

type Color = Blue | Red | Green | Orange | Yellow | Purple

type Hex = {
    NE : Color // Northeast
    E : Color // East
    SE : Color // Southeast
    SW : Color // Southwest
    W : Color // West
    NW : Color // Northwest
}

let hexes : Hex list = [
    {
        NE = Blue
        E = Red
        SE = Green
        SW = Orange
        W = Yellow
        NW = Purple
    }
    {
        NE = Orange
        E = Purple
        SE = Yellow
        SW = Red
        W = Blue
        NW = Green
    }
    {
        NE = Red
        E = Blue
        SE = Yellow
        SW = Orange
        W = Purple
        NW = Green
    }
    {
        NE = Red
        E = Green
        SE = Blue
        SW = Purple
        W = Orange
        NW = Yellow
    }
    {
        NE = Orange
        E = Purple
        SE = Blue
        SW = Red
        W = Green
        NW = Yellow
    }
    {
        NE = Orange
        E = Green
        SE = Blue
        SW = Red
        W = Purple
        NW = Yellow
    }
]

type Board = {
    T : Hex // Top
    CL : Hex // Center left
    CR : Hex // Center right
    BL : Hex // Bottom left
    BC : Hex // Bottom center
    BR : Hex // Bottom right
}

let rotate_hex_clockwise (hex : Hex) : Hex =
    {
        NE = hex.NW
        E = hex.NE
        SE = hex.E
        SW = hex.SE
        W = hex.SW
        NW = hex.W
    }

let get_all_hex_rotations (hex_1 : Hex) : Hex list =
    (hex_1, 6) |> List.unfold (fun (hex_2, remaining_rotations) ->
        if 0 = remaining_rotations then None
        else (hex_2, (rotate_hex_clockwise hex_2, remaining_rotations - 1)) |> Some
    )

let place_hexes_on_board (hexes : Hex list) : Board =
    {
        T = hexes.[0]
        CL = hexes.[1]
        CR = hexes.[2]
        BL = hexes.[3]
        BC = hexes.[4]
        BR = hexes.[5]
    }

let check_board (board : Board) : bool =
    board.T.SW = board.CL.NE &&
    board.T.SE = board.CR.NW &&
    board.CL.SW = board.BL.NE &&
    board.CL.SE = board.BC.NW &&
    board.CL.E = board.CR.W &&
    board.CR.SW = board.BC.NE &&
    board.CR.SE = board.BR.NW &&
    board.BL.E = board.BC.W &&
    board.BC.E = board.BR.W

/// Distribute x among each list in xs.
/// For example, for x = 1 and xs =
/// [
///   [2;3]
///   [4;5]
/// ]
/// The result is:
/// [
///   [1;2;3]
///   [1;4;5]
/// ]
/// 
let distribute (x : 'a) (xs : 'a list list) : 'a list list =
    let cons x xs = x :: xs
    if List.isEmpty xs then [[x]]
    else List.map (cons x) xs

/// Return all combinations of one item from each list in xs.
/// For example, for xs =
/// [
///   [1;2]
///   [3;4]
/// ]
/// The result is:
/// [
///   [1;3]
///   [1;4]
///   [2;3]
///   [2;4]
/// ]
/// 
let rec combine (xs : 'a list list) : 'a list list =
    match xs with
    | [] -> []
    | hd :: tl -> List.collect (fun i -> distribute i (combine tl)) hd

(* This works as follows.
For example, for xs = [1;2;3]:

1. Start with empty accumulator [[]] and xs first item 1.
2. Insert xs first item 1 at every index in every sublist in accumulator from 0 to sublist.Length. Each insertion yields a new sublist.
3. Accumulator is now [[[1]]].
4. List.collect reduces accumulator to [[1]].
5. Insert xs next item 2 at every index in every sublist in accumulator from 0 to sublist.Length. Each insertion yields a new sublist.
6. Accumulator is now [[[2;1]]; [[1;2]]].
7. List.collect reduces accumulator to [[2;1]; [1;2]].
8. Insert xs next item 3 at every index in every sublist in accumulator from 0 to sublist.Length. Each insertion yields a new sublist.
9. Accumulator is now [[[3;2;1]; [2;3;1]; [2;1;3]]; [[3;1;2]; [1;3;2]; [1;2;3]]].
10. List.collect reduces accumulator to [[3;2;1]; [2;3;1]; [2;1;3]; [3;1;2]; [1;3;2]; [1;2;3]].
11. No more items in xs, so return final accumulator state.

See:
https://dev.to/ducaale/computing-permutations-of-a-list-in-f-1n6k
*)
/// Return all permutations of xs.
/// For example, for xs:
/// [1;2;3]
/// The result is:
/// [
///   [1;2;3]
///   [1;3;2]
///   [2;1;3]
///   [2;3;1]
///   [3;1;2]
///   [3;2;1]
/// ]
/// 
let permute (xs_1 : 'a list) : 'a list list =
    let insert_at_all_indices (x : 'a) (xs_2 : 'a list) =
       [ for i in 0 .. xs_2.Length -> List.insertAt i x xs_2 ]
    List.fold (fun acc x -> List.collect (insert_at_all_indices x) acc) [[]] xs_1

(*
1. Start with list of hexes.

2. For each hex, generate all rotations (get_all_hex_rotations). For example:
[
  [H1R0;H1R1;H1R2;H1R3;H1R4;H1R5]
  [H2R0;H2R1;H2R2;H2R3;H2R4;H2R5]
  ...
  [H6R0;H6R1;H6R2;H6R3;H6R4;H6R5]
]

3. Get all combinations of hexes and rotations. For example:
[
  [H1R0;H1R1;...]
  [H2R0;H2R1;...]
  ...
  [H6R0;H6R1;...]
]
Becomes:
[
  [H1R0;H2R0;...H6R0]
  [H1R0;H2R0;...H6R1]
  ...
  [H1R5;H2R5;...H6R5]
]

4. For each combination, get all permutations. For example:
[H1R0;H2R0;H3R0;...]
becomes
[
  [H1R0;H2R0;H3R0;...]
  [H1R0;H3R0;H2R0;...]
  [H2R0;H1R0;H3R0;...]
  [H2R0;H3R0;H1R0;...]
  ...
]

5. For each permutation, place the hexes on the board.

6. For each board placement, check if it is a solution (that is, all facing colors match).

7. Print each solution.
*)

do hexes // Step 1
    |> List.map get_all_hex_rotations // Step 2
    |> combine // Step 3
    |> Seq.collect permute // Step 4
    |> Seq.map place_hexes_on_board // Step 5
    |> Seq.filter check_board // Step 6
    |> Seq.iter (fun solution -> printfn "%A%s" solution Environment.NewLine) // Step 7

(* Solutions: 6 out of 33,592,320 possible:

We can place any of 6 hexes in slot 1, with any of 6 rotations (36).
Then we can place any of the remaining 5 hexes in slot 2, with any of 6 rotations (30).
And so on.
(6 * 6) * (6 * 5) * (6 * 4) * (6 * 3) * (6 * 2) * 6 = 33,592,320

{ T = { NE = Blue
        E = Red
        SE = Green
        SW = Orange
        W = Yellow
        NW = Purple }
  CL = { NE = Orange
         E = Purple
         SE = Yellow
         SW = Red
         W = Blue
         NW = Green }
  CR = { NE = Red
         E = Blue
         SE = Yellow
         SW = Orange
         W = Purple
         NW = Green }
  BL = { NE = Red
         E = Green
         SE = Blue
         SW = Purple
         W = Orange
         NW = Yellow }
  BC = { NE = Orange
         E = Purple
         SE = Blue
         SW = Red
         W = Green
         NW = Yellow }
  BR = { NE = Orange
         E = Green
         SE = Blue
         SW = Red
         W = Purple
         NW = Yellow } }

{ T = { NE = Yellow
        E = Orange
        SE = Purple
        SW = Green
        W = Red
        NW = Blue }
  CL = { NE = Green
         E = Blue
         SE = Red
         SW = Purple
         W = Yellow
         NW = Orange }
  CR = { NE = Orange
         E = Yellow
         SE = Red
         SW = Green
         W = Blue
         NW = Purple }
  BL = { NE = Purple
         E = Blue
         SE = Red
         SW = Green
         W = Orange
         NW = Yellow }
  BC = { NE = Green
         E = Yellow
         SE = Orange
         SW = Purple
         W = Blue
         NW = Red }
  BR = { NE = Blue
         E = Green
         SE = Orange
         SW = Purple
         W = Yellow
         NW = Red } }

{ T = { NE = Orange
        E = Yellow
        SE = Red
        SW = Green
        W = Blue
        NW = Purple }
  CL = { NE = Green
         E = Yellow
         SE = Orange
         SW = Purple
         W = Blue
         NW = Red }
  CR = { NE = Blue
         E = Green
         SE = Orange
         SW = Purple
         W = Yellow
         NW = Red }
  BL = { NE = Purple
         E = Yellow
         SE = Orange
         SW = Green
         W = Blue
         NW = Red }
  BC = { NE = Purple
         E = Green
         SE = Red
         SW = Blue
         W = Yellow
         NW = Orange }
  BR = { NE = Yellow
         E = Purple
         SE = Blue
         SW = Red
         W = Green
         NW = Orange } }

{ T = { NE = Orange
        E = Yellow
        SE = Purple
        SW = Blue
        W = Red
        NW = Green }
  CL = { NE = Blue
         E = Red
         SE = Green
         SW = Yellow
         W = Orange
         NW = Purple }
  CR = { NE = Yellow
         E = Orange
         SE = Green
         SW = Blue
         W = Red
         NW = Purple }
  BL = { NE = Yellow
         E = Red
         SE = Blue
         SW = Green
         W = Orange
         NW = Purple }
  BC = { NE = Blue
         E = Purple
         SE = Orange
         SW = Yellow
         W = Red
         NW = Green }
  BR = { NE = Red
         E = Blue
         SE = Yellow
         SW = Orange
         W = Purple
         NW = Green } }

{ T = { NE = Blue
        E = Red
        SE = Purple
        SW = Yellow
        W = Orange
        NW = Green }
  CL = { NE = Yellow
         E = Orange
         SE = Purple
         SW = Green
         W = Red
         NW = Blue }
  CR = { NE = Blue
         E = Red
         SE = Green
         SW = Yellow
         W = Orange
         NW = Purple }
  BL = { NE = Green
         E = Orange
         SE = Yellow
         SW = Purple
         W = Blue
         NW = Red }
  BC = { NE = Yellow
         E = Red
         SE = Blue
         SW = Green
         W = Orange
         NW = Purple }
  BR = { NE = Blue
         E = Purple
         SE = Orange
         SW = Yellow
         W = Red
         NW = Green } }

{ T = { NE = Orange
        E = Purple
        SE = Yellow
        SW = Red
        W = Blue
        NW = Green }
  CL = { NE = Red
         E = Green
         SE = Blue
         SW = Purple
         W = Orange
         NW = Yellow }
  CR = { NE = Orange
         E = Purple
         SE = Blue
         SW = Red
         W = Green
         NW = Yellow }
  BL = { NE = Purple
         E = Green
         SE = Red
         SW = Blue
         W = Yellow
         NW = Orange }
  BC = { NE = Red
         E = Purple
         SE = Yellow
         SW = Orange
         W = Green
         NW = Blue }
  BR = { NE = Red
         E = Green
         SE = Orange
         SW = Yellow
         W = Purple
         NW = Blue } }
*)
