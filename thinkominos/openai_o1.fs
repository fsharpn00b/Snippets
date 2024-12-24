open System

/// We have 6 dominos, each with 6 colors in clockwise order, starting from the NE edge.
/// For clarity, store them in an array of arrays:
let allDominos =
    [|
        // Domino 1
        [| "blue"; "red";    "green"; "orange"; "yellow"; "purple" |]
        // Domino 2
        [| "orange"; "purple"; "yellow"; "red";   "blue";   "green" |]
        // Domino 3
        [| "red";   "blue";   "yellow"; "orange"; "purple"; "green" |]
        // Domino 4
        [| "red";   "green";  "blue";   "purple"; "orange"; "yellow"|]
        // Domino 5
        [| "orange"; "purple"; "blue";   "red";    "green";  "yellow"|]
        // Domino 6
        [| "orange"; "green";  "blue";   "red";    "purple"; "yellow"|]
    |]

/// A rotation by `r` steps means 'rotate the array by r places clockwise'.
/// For example, rotateDomino domino 1 means a 60° clockwise turn
/// (shift by 1 in the array).
let rotateDomino (domino: string[]) (r: int) =
    let n = domino.Length
    Array.init n (fun i -> domino.[(i - r + n) % n])

/// Check adjacency for this puzzle’s “pyramid” arrangement.
/// Given an array of 6 “placed and rotated” dominos, in the order:
///  0 = top
///  1 = center-left
///  2 = center-right
///  3 = bottom-left
///  4 = bottom-center
///  5 = bottom-right
/// Return true if all matching edges are color-consistent.
let checkAdjacency (placed: string[][]) =
    // placed.[i].[0] = NE, = E, = SE, = SW, = W, = NW

    // 1) top -> center-left: top SW (3) == center-left NE (0)
    if placed.[0].[3] <> placed.[1].[0] then false
    // 2) top -> center-right: top SE (2) == center-right NW (5)
    elif placed.[0].[2] <> placed.[2].[5] then false
    // 3) center-left -> center-right: left E (1) == right W (4)
    elif placed.[1].[1] <> placed.[2].[4] then false
    // 4) center-left -> bottom-left: left SW (3) == bot-left NE (0)
    elif placed.[1].[3] <> placed.[3].[0] then false
    // 5) center-left -> bottom-center: left SE (2) == bot-center NW (5)
    elif placed.[1].[2] <> placed.[4].[5] then false
    // 6) center-right -> bottom-center: right SW (3) == bot-center NE (0)
    elif placed.[2].[3] <> placed.[4].[0] then false
    // 7) center-right -> bottom-right: right SE (2) == bot-right NW (5)
    elif placed.[2].[2] <> placed.[5].[5] then false
    // 8) bottom-center -> bottom-left: center W (4) == left E (1)
    elif placed.[4].[4] <> placed.[3].[1] then false
    // 9) bottom-center -> bottom-right: center E (1) == right W (4)
    elif placed.[4].[1] <> placed.[5].[4] then false
    else true

/// Generate all permutations of a list. A simple recursive approach
let rec permutations xs =
    match xs with
    | [] -> seq [ [] ]
    | _  ->
        seq {
            for i in 0 .. (List.length xs - 1) do
                let x = xs.[i]
                let rest = List.removeAt i xs
                for perm in permutations rest do
                    yield x :: perm
        }

[<EntryPoint>]
let main _ =

    // We label dominos 0..5; generate all permutations of [0..5].
    let dominosIndices = [0;1;2;3;4;5]

    // We’ll store each successful placement in a list, just to demonstrate.
    let mutable solutions = []

    // Try each permutation of the 6 distinct dominos into the 6 pyramid slots.
    for perm in permutations dominosIndices do
        // perm is something like [2;4;5;0;3;1], meaning “Domino2 in top,
        // Domino4 in center‐left, Domino5 in center‐right, …”

        // Now we must try all 6^6 possible orientation combinations.
        // orientation[i] is 0..5, the rotation for the ith domino in the pyramid.
        let rec loopOrientation (slot: int) (current: string[][]) =
            if slot = 6 then
                // Check adjacency
                if checkAdjacency current then
                    solutions <- (perm, current) :: solutions
            else
                // Try each rotation 0..5
                for r in 0 .. 5 do
                    let rotated = rotateDomino allDominos.[perm.[slot]] r
                    let updated = Array.copy current
                    updated.[slot] <- rotated
                    loopOrientation (slot + 1) updated

        loopOrientation 0 (Array.create 6 [||])

    printfn "Found %d solutions." solutions.Length

    // Optionally print a few solutions
    // (Beware this might be large if many solutions exist!)
    solutions
    |> Seq.iteri (fun idx (placement, rots) ->
        printfn "Solution #%d: \n  Permutation = %A" (idx+1) placement
        for i in 0..5 do
            printfn "    Slot %d -> %A" i rots.[i]
        printfn ""
    )

    0 // exit code
