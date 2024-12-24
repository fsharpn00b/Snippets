# Thinkominos

This project shows how to solve the Pressman Thinkominos puzzle from 1984 using F#.

![Cover](https://i.postimg.cc/5NZ4xhMT/IMG-20240616-043336241.jpg)

![One of six solutions](https://i.postimg.cc/pdvPjKrB/IMG-20240616-043403791.jpg)

## Update 20241224

I asked OpenAI o1 to solve this puzzle as follows:

```
I have six dominoes.

Each domino has six sides: northeast, east, southeast, southwest, west, northwest.

Each side of each domino has a color.
Domino 1 has the following colors, starting from the northeast side and going clockwise:
    blue, red, green, orange, yellow, purple.
Domino 2 has the following colors, starting from the northeast side and going clockwise:
    orange, purple, yellow, red, blue, green. 
Domino 3 has the following colors, starting from the northeast side and going clockwise:
    red, blue, yellow, orange, purple, green.
Domino 4 has the following colors, starting from the northeast side and going clockwise:
    red, green, blue, purple, orange, yellow.
Domino 5 has the following colors, starting from the northeast side and going clockwise:
    orange, purple, blue, red, green, yellow.
Domino 6 has the following colors, starting from the northeast side and going clockwise:
    orange, green, blue, red, purple, yellow.

The dominos must be placed in a pyramid. The pyramid consists of six locations:
    top, center left, center right, bottom left, bottom center, bottom right.

Each domino can be rotated any number of times, either clockwise or counterclockwise, before being placed in the pyramid.
For example, if you rotate domino 1 one time clockwise, it now has the following colors, starting from the northeast side
and going clockwise: purple, blue, red, green, orange, yellow.

For the domino in the top location, the southwest side faces the northeast side of the domino in the center left location,
and the southeast side faces the northwest side of the domino in the center right location.
For the domino in the center left location, the east side faces the west side of the domino in the center right location,
the southwest side faces the northeast side of the domino in the bottom left location, and the southeast side faces the
northwest side of the domino in the bottom center location.
For the domino in the center right location, the southwest side faces the northeast side of the domino in the bottom center
location, and the southeast side faces the northwest side of the domino in the bottom right location.
For the domino in the bottom center location, the west side faces the east side of the domino in the bottom left location,
and the east side faces the west side of the domino in the bottom right location.

A solution consists of rotating and placing all six dominos in the pyramid such that, for each side of each domino that
faces a side of another domino, the facing sides have the same color.

As an example, here is one correct solution:

Top: Domino 1, rotated 0 times.
Center left: Domino 2, rotated 0 times.
Center right: Domino 3, rotated 0 times.
Bottom left: Domino 4, rotated 0 times.
Bottom center: Domino 5, rotated 0 times.
Bottom right: Domino 6, rotated 0 times.

Please find all possible solutions.
```

o1 replied:

```
After examining all ways of permuting and rotating the six dominos so that adjoining edges match in color, it turns out
there is effectively only one placement-and-rotation scheme (up to relabeling the same arrangement) that satisfies all
of the adjacency requirements. In other words, the example arrangement you gave (Domino 1 on top, Domino 2 center left,
Domino 3 center right, Domino 4 bottom left, Domino 5 bottom center, Domino 6 bottom right, all with zero rotations) is
the unique solution.
```

I replied that multiple solutions exist, and sent it the picture shown above.

o1 replied:

```
[T]here are indeed multiple ways to seat and rotate these six hex‐dominoes so that every shared edge matches in color—
which confirms that the photo you attached does show a valid (i.e., different) solution. Unfortunately, none of the
search snippets above appear to discuss “Thinkominos” or an equivalent six‐piece color‐matching puzzle directly, so
there is no relevant external citation to make here.
...
HOW TO FIND ALL SUCH SOLUTIONS

A common way is to do a systematic (often computer‐assisted) search:
• Permute the six dominos into the six “slots” (top, center‐left, etc.).
• For each permutation, test all 6⁶ ways to rotate the dominos (6 possible orientations per piece).
• Whenever all contacting edges match, record that arrangement.
Due to symmetries (for instance, flipping the entire pyramid left‐to‐right), some solutions end up “equivalent,” but
they do look different once laid out on the table. By enumerating them carefully, one can confirm there are multiple
fundamentally distinct ways to solve the puzzle.
```

The last is an interesting observation given you can indeed go from solution to another by simply rotating the entire
pyramid 120 or 240 degrees.

I asked it:

```
Can you please create a program in F# for method 3?
```

It replied with the program in [openai_o1.fs](openai_o1.fs). I had to do some cleanup. For instance, the
checkAdjacency function was missing the array indices, but I was able to recreate them from the comments. But once
cleaned up, the program ran and output the following solutions that agree with mine:

```
Solution #1: 
  Permutation = [5; 2; 4; 0; 1; 3]
    Slot 0 -> [|"blue"; "red"; "purple"; "yellow"; "orange"; "green"|]
    Slot 1 -> [|"yellow"; "orange"; "purple"; "green"; "red"; "blue"|]
    Slot 2 -> [|"blue"; "red"; "green"; "yellow"; "orange"; "purple"|]
    Slot 3 -> [|"green"; "orange"; "yellow"; "purple"; "blue"; "red"|]
    Slot 4 -> [|"yellow"; "red"; "blue"; "green"; "orange"; "purple"|]
    Slot 5 -> [|"blue"; "purple"; "orange"; "yellow"; "red"; "green"|]

Solution #2: 
  Permutation = [3; 4; 1; 5; 2; 0]
    Slot 0 -> [|"orange"; "yellow"; "red"; "green"; "blue"; "purple"|]
    Slot 1 -> [|"green"; "yellow"; "orange"; "purple"; "blue"; "red"|]
    Slot 2 -> [|"blue"; "green"; "orange"; "purple"; "yellow"; "red"|]
    Slot 3 -> [|"purple"; "yellow"; "orange"; "green"; "blue"; "red"|]
    Slot 4 -> [|"purple"; "green"; "red"; "blue"; "yellow"; "orange"|]
    Slot 5 -> [|"yellow"; "purple"; "blue"; "red"; "green"; "orange"|]

Solution #3: 
  Permutation = [2; 5; 3; 0; 4; 1]
    Slot 0 -> [|"yellow"; "orange"; "purple"; "green"; "red"; "blue"|]
    Slot 1 -> [|"green"; "blue"; "red"; "purple"; "yellow"; "orange"|]
    Slot 2 -> [|"orange"; "yellow"; "red"; "green"; "blue"; "purple"|]
    Slot 3 -> [|"purple"; "blue"; "red"; "green"; "orange"; "yellow"|]
    Slot 4 -> [|"green"; "yellow"; "orange"; "purple"; "blue"; "red"|]
    Slot 5 -> [|"blue"; "green"; "orange"; "purple"; "yellow"; "red"|]

Solution #4: 
  Permutation = [1; 3; 4; 2; 5; 0]
    Slot 0 -> [|"orange"; "purple"; "yellow"; "red"; "blue"; "green"|]
    Slot 1 -> [|"red"; "green"; "blue"; "purple"; "orange"; "yellow"|]
    Slot 2 -> [|"orange"; "purple"; "blue"; "red"; "green"; "yellow"|]
    Slot 3 -> [|"purple"; "green"; "red"; "blue"; "yellow"; "orange"|]
    Slot 4 -> [|"red"; "purple"; "yellow"; "orange"; "green"; "blue"|]
    Slot 5 -> [|"red"; "green"; "orange"; "yellow"; "purple"; "blue"|]

Solution #5: 
  Permutation = [0; 4; 5; 1; 3; 2]
    Slot 0 -> [|"orange"; "yellow"; "purple"; "blue"; "red"; "green"|]
    Slot 1 -> [|"blue"; "red"; "green"; "yellow"; "orange"; "purple"|]
    Slot 2 -> [|"yellow"; "orange"; "green"; "blue"; "red"; "purple"|]
    Slot 3 -> [|"yellow"; "red"; "blue"; "green"; "orange"; "purple"|]
    Slot 4 -> [|"blue"; "purple"; "orange"; "yellow"; "red"; "green"|]
    Slot 5 -> [|"red"; "blue"; "yellow"; "orange"; "purple"; "green"|]

Solution #6: 
  Permutation = [0; 1; 2; 3; 4; 5]
    Slot 0 -> [|"blue"; "red"; "green"; "orange"; "yellow"; "purple"|]
    Slot 1 -> [|"orange"; "purple"; "yellow"; "red"; "blue"; "green"|]
    Slot 2 -> [|"red"; "blue"; "yellow"; "orange"; "purple"; "green"|]
    Slot 3 -> [|"red"; "green"; "blue"; "purple"; "orange"; "yellow"|]
    Slot 4 -> [|"orange"; "purple"; "blue"; "red"; "green"; "yellow"|]
    Slot 5 -> [|"orange"; "green"; "blue"; "red"; "purple"; "yellow"|]
```
